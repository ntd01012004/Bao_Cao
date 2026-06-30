using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AI Resume & Career Recommendation Demo API",
        Version = "v1",
        Description = "Demo AI Resume Generation, Resume Template Generation, ATS Optimization, Personalized Resume Generation, Academic-to-Career Mapping and Career Recommendation System."
    });
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Resume & Career Demo API v1");
    options.RoutePrefix = "swagger";
});

var data = DemoData.Create();

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "Healthy",
    app = "AI Resume & Career Recommendation Demo",
    generatedAt = DateTimeOffset.Now
}))
.WithTags("System")
.WithSummary("Check API health");

app.MapGet("/api/demo/sample", () => Results.Ok(data.SampleRequest))
    .WithTags("Demo Data")
    .WithSummary("Get sample student profile and job description");

app.MapGet("/api/demo/samples", () => Results.Ok(data.Samples))
    .WithTags("Demo Data")
    .WithSummary("Get diverse sample candidate profiles");

app.MapGet("/api/demo/samples/{index:int}", (int index) =>
{
    if (index < 0 || index >= data.Samples.Count)
    {
        return Results.NotFound(new { message = $"Sample index {index} was not found." });
    }

    return Results.Ok(data.Samples[index]);
})
.WithTags("Demo Data")
.WithSummary("Get one sample candidate profile by index");

app.MapGet("/api/skills/taxonomy", () => Results.Ok(data.SkillTaxonomy))
    .WithTags("Skill Mapping")
    .WithSummary("Get normalized skill taxonomy");

app.MapPost("/api/job/analyze", (JobAnalyzeRequest request) =>
{
    var job = JobDescriptionAnalyzer.Analyze(request.JobTitle, request.JobDescription, data.SkillTaxonomy);
    return Results.Ok(job);
})
.WithTags("Job Description Analyzer")
.WithSummary("Extract required skills and keywords from a job description");

app.MapPost("/api/academic-career/map", (AcademicCareerRequest request) =>
{
    var result = AcademicCareerMapper.Map(request, data.AcademicRules, data.CareerRoles);
    return Results.Ok(result);
})
.WithTags("Academic-to-Career Mapping")
.WithSummary("Map courses, projects and activities to career skills and roles");

app.MapPost("/api/resume/templates/generate", (TemplateRequest request) =>
{
    var template = ResumeTemplateEngine.Generate(request);
    return Results.Ok(template);
})
.WithTags("Resume Template Generation")
.WithSummary("Generate an ATS-friendly resume template structure");

app.MapPost("/api/resume/generate", (ResumeGenerationRequest request) =>
{
    var normalizedSkills = SkillNormalizer.NormalizeMany(request.Profile.Skills, data.SkillTaxonomy);
    var academicMap = AcademicCareerMapper.Map(new AcademicCareerRequest(
        request.Profile.Major,
        request.Profile.Courses,
        request.Profile.Projects,
        request.Profile.Activities), data.AcademicRules, data.CareerRoles);
    var job = JobDescriptionAnalyzer.Analyze(request.TargetJobTitle, request.JobDescription, data.SkillTaxonomy);
    var resume = ResumeGenerator.Generate(request, normalizedSkills, academicMap, job);

    return Results.Ok(resume);
})
.WithTags("AI Resume Generation")
.WithSummary("Generate a resume draft from real user-provided profile data");

app.MapPost("/api/resume/personalize", (ResumeGenerationRequest request) =>
{
    var normalizedSkills = SkillNormalizer.NormalizeMany(request.Profile.Skills, data.SkillTaxonomy);
    var academicMap = AcademicCareerMapper.Map(new AcademicCareerRequest(
        request.Profile.Major,
        request.Profile.Courses,
        request.Profile.Projects,
        request.Profile.Activities), data.AcademicRules, data.CareerRoles);
    var job = JobDescriptionAnalyzer.Analyze(request.TargetJobTitle, request.JobDescription, data.SkillTaxonomy);
    var resume = ResumeGenerator.Generate(request with { PreferOnePage = true }, normalizedSkills, academicMap, job);
    var ats = AtsOptimizer.Evaluate(resume, job);

    return Results.Ok(new PersonalizedResumeResult(
        resume,
        ats,
        ats.MissingSkills.Select(skill => $"N\u1ebfu b\u1ea1n th\u1ef1c s\u1ef1 c\u00f3 kinh nghi\u1ec7m v\u1ec1 {skill}, h\u00e3y th\u00eam b\u1eb1ng ch\u1ee9ng v\u00e0o ph\u1ea7n Projects ho\u1eb7c Skills.").ToList(),
        "B\u1ea3n c\u00e1 nh\u00e2n h\u00f3a n\u00e0y \u01b0u ti\u00ean k\u1ef9 n\u0103ng v\u00e0 d\u1ef1 \u00e1n li\u00ean quan tr\u1ef1c ti\u1ebfp \u0111\u1ebfn m\u00f4 t\u1ea3 c\u00f4ng vi\u1ec7c."));
})
.WithTags("Personalized Resume Generation")
.WithSummary("Generate a job-specific personalized resume");

app.MapPost("/api/ats/optimize", (AtsRequest request) =>
{
    var job = JobDescriptionAnalyzer.Analyze(request.JobTitle, request.JobDescription, data.SkillTaxonomy);
    var result = AtsOptimizer.Evaluate(request.Resume, job);
    return Results.Ok(result);
})
.WithTags("ATS Resume Optimization")
.WithSummary("Calculate ATS score, matched skills, missing skills and improvement advice");

app.MapPost("/api/careers/recommend", (CareerRecommendationRequest request) =>
{
    var normalizedSkills = SkillNormalizer.NormalizeMany(request.Profile.Skills, data.SkillTaxonomy);
    var academicMap = AcademicCareerMapper.Map(new AcademicCareerRequest(
        request.Profile.Major,
        request.Profile.Courses,
        request.Profile.Projects,
        request.Profile.Activities), data.AcademicRules, data.CareerRoles);
    var result = CareerRecommendationEngine.Recommend(request.Profile, normalizedSkills, academicMap, data.CareerRoles);
    return Results.Ok(result);
})
.WithTags("Career Recommendation System")
.WithSummary("Recommend suitable career paths and skill gaps");

app.MapPost("/api/pipeline/run", (ResumeGenerationRequest request) =>
{
    var normalizedSkills = SkillNormalizer.NormalizeMany(request.Profile.Skills, data.SkillTaxonomy);
    var academicMap = AcademicCareerMapper.Map(new AcademicCareerRequest(
        request.Profile.Major,
        request.Profile.Courses,
        request.Profile.Projects,
        request.Profile.Activities), data.AcademicRules, data.CareerRoles);
    var job = JobDescriptionAnalyzer.Analyze(request.TargetJobTitle, request.JobDescription, data.SkillTaxonomy);
    var template = ResumeTemplateEngine.Generate(new TemplateRequest(request.TargetJobTitle, request.Profile.ExperienceLevel, true, request.Language));
    var resume = ResumeGenerator.Generate(request, normalizedSkills, academicMap, job);
    var ats = AtsOptimizer.Evaluate(resume, job);
    var careers = CareerRecommendationEngine.Recommend(request.Profile, normalizedSkills, academicMap, data.CareerRoles);

    return Results.Ok(new PipelineResult(job, academicMap, template, resume, ats, careers));
})
.WithTags("End-to-End Demo")
.WithSummary("Run the full architecture flow from profile and JD to final resume advice");

app.MapFallbackToFile("index.html");

app.Run();

static class SkillNormalizer
{
    public static List<string> NormalizeMany(IEnumerable<string> skills, IReadOnlyList<SkillTaxonomyItem> taxonomy)
    {
        return skills
            .Select(skill => Normalize(skill, taxonomy))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(skill => skill)
            .ToList();
    }

    public static string Normalize(string rawSkill, IReadOnlyList<SkillTaxonomyItem> taxonomy)
    {
        var value = Clean(rawSkill);
        var match = taxonomy.FirstOrDefault(item =>
            Clean(item.Name) == value ||
            item.Aliases.Any(alias => Clean(alias) == value) ||
            value.Contains(Clean(item.Name), StringComparison.OrdinalIgnoreCase));

        return match?.Name ?? rawSkill.Trim();
    }

    public static bool ContainsSkill(string text, SkillTaxonomyItem skill)
    {
        var clean = Clean(text);
        return clean.Contains(Clean(skill.Name), StringComparison.OrdinalIgnoreCase) ||
               skill.Aliases.Any(alias => clean.Contains(Clean(alias), StringComparison.OrdinalIgnoreCase));
    }

    static string Clean(string value)
    {
        return TextSearch.RemoveDiacritics(value).Trim().ToLowerInvariant()
            .Replace(".", "")
            .Replace("-", " ")
            .Replace("_", " ");
    }
}

static class TextSearch
{
    public static bool Contains(string source, string value)
    {
        return RemoveDiacritics(source).Contains(RemoveDiacritics(value), StringComparison.OrdinalIgnoreCase);
    }

    public static string RemoveDiacritics(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace('\u0111', 'd')
            .Replace('\u0110', 'D');
    }
}

static class JobDescriptionAnalyzer{
    public static JobAnalysis Analyze(string jobTitle, string jobDescription, IReadOnlyList<SkillTaxonomyItem> taxonomy)
    {
        var text = $"{jobTitle} {jobDescription}";
        var required = taxonomy
            .Where(skill => SkillNormalizer.ContainsSkill(text, skill))
            .Select(skill => skill.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var seniority = text.Contains("intern", StringComparison.OrdinalIgnoreCase) ? "Intern/Fresher" :
            text.Contains("senior", StringComparison.OrdinalIgnoreCase) ? "Senior" :
            text.Contains("junior", StringComparison.OrdinalIgnoreCase) ? "Junior" : "General";

        var keywords = required
            .Concat(ExtractImportantWords(jobDescription))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(16)
            .ToList();

        return new JobAnalysis(jobTitle, seniority, required, keywords, jobDescription);
    }

    static IEnumerable<string> ExtractImportantWords(string text)
    {
        string[] stopWords = ["and", "or", "with", "using", "the", "for", "can", "have", "has", "la", "va", "voi", "yeu", "cau"];
        return text.Split([' ', ',', '.', ';', ':', '\n', '\r', '/', '(', ')'], StringSplitOptions.RemoveEmptyEntries)
            .Select(word => word.Trim())
            .Where(word => word.Length > 3 && !stopWords.Contains(word.ToLowerInvariant()))
            .Take(20);
    }
}

static class AcademicCareerMapper
{
    public static AcademicCareerResult Map(AcademicCareerRequest request, IReadOnlyList<AcademicRule> rules, IReadOnlyList<CareerRole> roles)
    {
        var evidence = new List<SkillEvidence>();
        var sources = request.Courses.Concat(request.Projects).Concat(request.Activities).ToList();

        foreach (var source in sources)
        {
            foreach (var rule in rules.Where(rule => TextSearch.Contains(source, rule.Trigger)))
            {
                evidence.Add(new SkillEvidence(source, rule.Skill, rule.EvidenceType, rule.ResumeSuggestion));
            }
        }

        var skills = evidence.Select(item => item.Skill).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(item => item).ToList();
        var roleMatches = roles.Select(role =>
        {
            var matched = role.RequiredSkills.Intersect(skills, StringComparer.OrdinalIgnoreCase).ToList();
            var score = role.RequiredSkills.Count == 0 ? 0 : Math.Round(matched.Count * 100m / role.RequiredSkills.Count, 1);
            var gaps = role.RequiredSkills.Except(skills, StringComparer.OrdinalIgnoreCase).ToList();
            return new AcademicRoleMatch(role.Name, score, matched, gaps);
        })
        .OrderByDescending(item => item.MatchScore)
        .ThenBy(item => item.RoleName)
        .Take(5)
        .ToList();

        return new AcademicCareerResult(request.Major, skills, evidence, roleMatches);
    }
}

static class ResumeTemplateEngine
{
    public static ResumeTemplate Generate(TemplateRequest request)
    {
        var isStudent = request.ExperienceLevel.Equals("Student", StringComparison.OrdinalIgnoreCase) ||
                        request.ExperienceLevel.Equals("Fresher", StringComparison.OrdinalIgnoreCase) ||
                        request.ExperienceLevel.Equals("Intern", StringComparison.OrdinalIgnoreCase);

        var sections = isStudent
            ? new List<string> { "Contact Information", "Career Objective", "Technical Skills", "Projects", "Education", "Certifications", "Activities" }
            : new List<string> { "Contact Information", "Professional Summary", "Work Experience", "Technical Skills", "Projects", "Education", "Certifications" };

        var warnings = new List<string>
        {
            "D\u00f9ng ti\u00eau \u0111\u1ec1 ph\u1ea7n ph\u1ed5 bi\u1ebfn \u0111\u1ec3 ATS d\u1ec5 ph\u00e2n t\u00edch.",
            "Tr\u00e1nh b\u1ea3ng ph\u1ee9c t\u1ea1p, icon l\u1ea1, textbox v\u00e0 b\u1ed1 c\u1ee5c qu\u00e1 nhi\u1ec1u c\u1ed9t.",
            "M\u1ed7i bullet n\u00ean c\u00f3 h\u00e0nh \u0111\u1ed9ng, c\u00f4ng ngh\u1ec7 v\u00e0 k\u1ebft qu\u1ea3 ho\u1eb7c ph\u1ea1m vi c\u00f4ng vi\u1ec7c."
        };

        return new ResumeTemplate(
            request.TargetJobTitle,
            request.AtsFriendly ? "ATS Friendly Single Column" : "Modern Simple",
            request.Language,
            request.AtsFriendly,
            sections,
            warnings);
    }
}

static class ResumeGenerator
{
    public static ResumeDraft Generate(ResumeGenerationRequest request, List<string> normalizedSkills, AcademicCareerResult academicMap, JobAnalysis job)
    {
        var profile = request.Profile;
        var sections = ResumeTemplateEngine.Generate(new TemplateRequest(request.TargetJobTitle, profile.ExperienceLevel, true, request.Language)).Sections;
        var matchedSkills = job.RequiredSkills.Intersect(normalizedSkills.Concat(academicMap.InferredSkills), StringComparer.OrdinalIgnoreCase).ToList();
        var missingSkills = job.RequiredSkills.Except(matchedSkills, StringComparer.OrdinalIgnoreCase).ToList();

        var summary = $"\u1ee8ng vi\u00ean ng\u00e0nh {profile.Major} \u0111\u1ecbnh h\u01b0\u1edbng {request.TargetJobTitle}, c\u00f3 kinh nghi\u1ec7m h\u1ecdc t\u1eadp v\u00e0 d\u1ef1 \u00e1n li\u00ean quan \u0111\u1ebfn {JoinReadable(matchedSkills.DefaultIfEmpty("ph\u00e1t tri\u1ec3n ngh\u1ec1 nghi\u1ec7p"))}. " +
                      "B\u1ea3n nh\u00e1p ch\u1ec9 s\u1eed d\u1ee5ng d\u1eef li\u1ec7u h\u1ed3 s\u01a1 \u0111\u00e3 nh\u1eadp, \u01b0u ti\u00ean b\u1eb1ng ch\u1ee9ng r\u00f5 r\u00e0ng v\u00e0 c\u00e1ch tr\u00ecnh b\u00e0y th\u00e2n thi\u1ec7n v\u1edbi ATS.";

        var projectBullets = profile.Projects.Select(project =>
        {
            var related = matchedSkills.Where(skill => project.Contains(skill, StringComparison.OrdinalIgnoreCase)).ToList();
            if (related.Count == 0)
            {
                related = matchedSkills.Take(3).ToList();
            }

            var skillText = related.Count == 0 ? "c\u00e1c k\u1ef9 n\u0103ng li\u00ean quan" : JoinReadable(related);
            return $"Tri\u1ec3n khai {project}, th\u1ec3 hi\u1ec7n r\u00f5 k\u1ef9 n\u0103ng {skillText}, ph\u1ea1m vi c\u00f4ng vi\u1ec7c, tr\u00e1ch nhi\u1ec7m c\u00e1 nh\u00e2n v\u00e0 k\u1ebft qu\u1ea3 h\u1ecdc \u0111\u01b0\u1ee3c.";
        }).ToList();

        if (projectBullets.Count == 0)
        {
            projectBullets.Add("Chuy\u1ec3n d\u1eef li\u1ec7u m\u00f4n h\u1ecdc th\u00e0nh b\u1eb1ng ch\u1ee9ng d\u1ef1 \u00e1n, n\u00eau r\u00f5 c\u00f4ng c\u1ee5 s\u1eed d\u1ee5ng, tr\u00e1ch nhi\u1ec7m v\u00e0 k\u1ebft qu\u1ea3 h\u1ecdc t\u1eadp \u0111o l\u01b0\u1eddng \u0111\u01b0\u1ee3c.");
        }

        var improvementNotes = missingSkills.Select(skill => $"Thi\u1ebfu ho\u1eb7c c\u00f2n y\u1ebfu b\u1eb1ng ch\u1ee9ng cho k\u1ef9 n\u0103ng {skill}. Ch\u1ec9 b\u1ed5 sung n\u1ebfu b\u1ea1n th\u1eadt s\u1ef1 c\u00f3 d\u1ef1 \u00e1n, m\u00f4n h\u1ecdc ho\u1eb7c ch\u1ee9ng ch\u1ec9 li\u00ean quan.").ToList();
        improvementNotes.Add("B\u1ea3n nh\u00e1p AI ch\u1ec9 \u0111\u01b0\u1ee3c sinh t\u1eeb d\u1eef li\u1ec7u \u0111\u00e3 cung c\u1ea5p; h\u00e3y ki\u1ec3m tra t\u1eebng c\u00e2u tr\u01b0\u1edbc khi xu\u1ea5t CV cu\u1ed1i c\u00f9ng.");

        return new ResumeDraft(
            profile.FullName,
            request.TargetJobTitle,
            summary,
            sections,
            normalizedSkills.Concat(academicMap.InferredSkills).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(skill => skill).ToList(),
            projectBullets,
            matchedSkills,
            missingSkills,
            improvementNotes);
    }

    static string JoinReadable(IEnumerable<string> values)
    {
        return string.Join(", ", values.Where(value => !string.IsNullOrWhiteSpace(value)).Take(6));
    }
}

static class AtsOptimizer
{
    public static AtsResult Evaluate(ResumeDraft resume, JobAnalysis job)
    {
        var resumeSkills = resume.Skills.Concat(resume.MatchedJobSkills).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var matched = job.RequiredSkills.Intersect(resumeSkills, StringComparer.OrdinalIgnoreCase).ToList();
        var missing = job.RequiredSkills.Except(matched, StringComparer.OrdinalIgnoreCase).ToList();

        var skillCoverage = job.RequiredSkills.Count == 0 ? 100m : Math.Round(matched.Count * 100m / job.RequiredSkills.Count, 1);
        var parseability = resume.Sections.All(section => section.Length <= 32) ? 92m : 78m;
        var keywordMatch = job.Keywords.Count == 0 ? 100m : Math.Round(job.Keywords.Count(keyword =>
            resume.Summary.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            resume.ProjectBullets.Any(bullet => bullet.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
            resume.Skills.Any(skill => skill.Contains(keyword, StringComparison.OrdinalIgnoreCase))) * 100m / job.Keywords.Count, 1);
        var formatSafety = resume.Sections.Contains("Technical Skills") && resume.Sections.Contains("Projects") ? 90m : 75m;
        var overall = Math.Round((skillCoverage * 0.45m) + (keywordMatch * 0.25m) + (parseability * 0.15m) + (formatSafety * 0.15m), 1);

        var advice = new List<string>
        {
            "Gi\u1eef b\u1ed1 c\u1ee5c m\u1ed9t c\u1ed9t, ti\u00eau \u0111\u1ec1 r\u00f5 r\u00e0ng v\u00e0 n\u1ed9i dung d\u1ea1ng text \u0111\u1ec3 ATS \u0111\u1ecdc \u0111\u01b0\u1ee3c.",
            "\u0110\u01b0a c\u00e1c k\u1ef9 n\u0103ng kh\u1edbp m\u00f4 t\u1ea3 c\u00f4ng vi\u1ec7c v\u00e0o ph\u1ea7n Skills v\u00e0 ch\u1ee9ng minh b\u1eb1ng Projects ho\u1eb7c Experience.",
            "Kh\u00f4ng nh\u1ed3i t\u1eeb kh\u00f3a n\u1ebfu kh\u00f4ng c\u00f3 b\u1eb1ng ch\u1ee9ng th\u1eadt."
        };
        advice.AddRange(missing.Select(skill => $"B\u1ed5 sung b\u1eb1ng ch\u1ee9ng cho k\u1ef9 n\u0103ng {skill} n\u1ebfu b\u1ea1n th\u1ef1c s\u1ef1 c\u00f3 n\u0103ng l\u1ef1c n\u00e0y."));

        return new AtsResult(overall, parseability, keywordMatch, skillCoverage, formatSafety, matched, missing, advice);
    }
}

static class CareerRecommendationEngine
{
    public static CareerRecommendationResult Recommend(StudentProfile profile, List<string> normalizedSkills, AcademicCareerResult academicMap, IReadOnlyList<CareerRole> roles)
    {
        var allSkills = normalizedSkills.Concat(academicMap.InferredSkills).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var recommendations = roles.Select(role =>
        {
            var matched = role.RequiredSkills.Intersect(allSkills, StringComparer.OrdinalIgnoreCase).ToList();
            var score = role.RequiredSkills.Count == 0 ? 0 : Math.Round(matched.Count * 100m / role.RequiredSkills.Count, 1);
            if (profile.CareerGoal.Contains(role.Name, StringComparison.OrdinalIgnoreCase))
            {
                score = Math.Min(100, score + 20);
            }

            var gaps = role.RequiredSkills.Except(allSkills, StringComparer.OrdinalIgnoreCase).ToList();
            var learningPath = gaps.Take(3).Select(skill => $"H\u1ecdc v\u00e0 l\u00e0m mini project c\u00f3 s\u1eed d\u1ee5ng {skill}.").ToList();
            return new CareerRecommendation(role.Name, score, matched, gaps, role.TemplateHint, learningPath);
        })
        .OrderByDescending(item => item.MatchScore)
        .ThenBy(item => item.RoleName)
        .Take(5)
        .ToList();

        return new CareerRecommendationResult(profile.FullName, recommendations);
    }
}

static class DemoData
{
    public static DemoDataset Create()
    {
        var taxonomy = new List<SkillTaxonomyItem>
        {
            new("C#", "Programming Language", ["csharp", "c sharp", ".net"]),
            new("SQL Server", "Database", ["sql", "mssql", "ms sql"]),
            new("REST API", "Backend", ["restful api", "web api", "api"]),
            new("Git", "Tool", ["version control", "github"]),
            new("OOP", "Programming Concept", ["object oriented programming", "lap trinh huong doi tuong"]),
            new("Web Development", "Web", ["website", "web application", "lap trinh web"]),
            new("HTML", "Frontend", ["html5"]),
            new("CSS", "Frontend", ["css3"]),
            new("Authentication", "Backend", ["login", "dang nhap", "auth"]),
            new("CRUD", "Backend", ["create read update delete"]),
            new("Product Management", "Domain", ["quan ly san pham"]),
            new("Database Design", "Database", ["data modeling", "schema", "relational database"]),
            new("Python", "Programming Language", ["py"]),
            new("Excel", "Tool", ["spreadsheet"]),
            new("Testing", "Quality", ["test case", "qa"]),
            new("Power BI", "Analytics", ["powerbi", "dashboard"]),
            new("Data Analysis", "Analytics", ["phan tich du lieu", "data analytics"]),
            new("Statistics", "Analytics", ["xac suat thong ke", "statistical thinking"]),
            new("Marketing", "Business", ["digital marketing", "marketing can ban"]),
            new("SEO", "Business", ["search engine optimization"]),
            new("Content Writing", "Business", ["copywriting", "viet noi dung"]),
            new("Figma", "Design", ["ui design", "ux design"]),
            new("User Research", "Design", ["ux research", "khao sat nguoi dung"]),
            new("Accounting", "Finance", ["ke toan", "financial accounting"]),
            new("Financial Analysis", "Finance", ["phan tich tai chinh"]),
            new("HR Operations", "Human Resources", ["nhan su", "tuyen dung"]),
            new("Communication", "Soft Skill", ["giao tiep", "presentation", "thuyet trinh"]),
            new("Teamwork", "Soft Skill", ["lam viec nhom", "collaboration"])
        };

        var academicRules = new List<AcademicRule>
        {
            new("Co so du lieu", "SQL Server", "Course", "N\u00eau r\u00f5 k\u1ef9 n\u0103ng thi\u1ebft k\u1ebf c\u01a1 s\u1edf d\u1eef li\u1ec7u, vi\u1ebft truy v\u1ea5n SQL v\u00e0 l\u00e0m vi\u1ec7c v\u1edbi schema quan h\u1ec7."),
            new("Co so du lieu", "Database Design", "Course", "M\u00f4 t\u1ea3 vi\u1ec7c thi\u1ebft k\u1ebf schema c\u01a1 s\u1edf d\u1eef li\u1ec7u quan h\u1ec7 cho \u0111\u1ed3 \u00e1n m\u00f4n h\u1ecdc."),
            new("Lap trinh huong doi tuong", "OOP", "Course", "M\u00f4 t\u1ea3 t\u01b0 duy l\u1eadp tr\u00ecnh h\u01b0\u1edbng \u0111\u1ed1i t\u01b0\u1ee3ng, thi\u1ebft k\u1ebf class v\u00e0 ph\u00e2n t\u00e1ch tr\u00e1ch nhi\u1ec7m."),
            new("Lap trinh huong doi tuong", "C#", "Course", "N\u00eau r\u00f5 ph\u1ea7n tri\u1ec3n khai b\u1eb1ng C# n\u1ebfu m\u00f4n h\u1ecdc ho\u1eb7c \u0111\u1ed3 \u00e1n c\u00f3 s\u1eed d\u1ee5ng."),
            new("Lap trinh web", "Web Development", "Course", "M\u00f4 t\u1ea3 vi\u1ec7c x\u00e2y d\u1ef1ng trang web ho\u1eb7c \u1ee9ng d\u1ee5ng web trong qu\u00e1 tr\u00ecnh h\u1ecdc."),
            new("Website ban hang", "Authentication", "Project", "\u0110\u01b0a module \u0111\u0103ng nh\u1eadp/x\u00e1c th\u1ef1c v\u00e0o ph\u1ea7n Projects."),
            new("Website ban hang", "CRUD", "Project", "N\u00eau r\u00f5 c\u00e1c ch\u1ee9c n\u0103ng CRUD cho s\u1ea3n ph\u1ea9m, \u0111\u01a1n h\u00e0ng ho\u1eb7c ng\u01b0\u1eddi d\u00f9ng."),
            new("gio hang", "Product Management", "Project", "M\u00f4 t\u1ea3 ch\u1ee9c n\u0103ng gi\u1ecf h\u00e0ng v\u00e0 qu\u1ea3n l\u00fd s\u1ea3n ph\u1ea9m."),
            new("nhom", "Git", "Activity", "N\u00eau r\u00f5 l\u00e0m vi\u1ec7c nh\u00f3m v\u00e0 qu\u1ea3n l\u00fd phi\u00ean b\u1ea3n c\u01a1 b\u1ea3n n\u1ebfu c\u00f3 d\u00f9ng Git."),
            new("Phan tich du lieu", "Data Analysis", "Course", "M\u00f4 t\u1ea3 vi\u1ec7c l\u00e0m s\u1ea1ch d\u1eef li\u1ec7u, ph\u00e2n t\u00edch xu h\u01b0\u1edbng v\u00e0 r\u00fat ra insight."),
            new("Xac suat thong ke", "Statistics", "Course", "G\u1eafn m\u00f4n h\u1ecdc v\u1edbi t\u01b0 duy th\u1ed1ng k\u00ea, \u0111o l\u01b0\u1eddng v\u00e0 ki\u1ec3m \u0111\u1ecbnh gi\u1ea3 thuy\u1ebft."),
            new("Excel", "Excel", "Tool", "N\u00eau r\u00f5 b\u1ea3ng t\u00ednh, pivot table, c\u00f4ng th\u1ee9c v\u00e0 b\u00e1o c\u00e1o d\u1eef li\u1ec7u."),
            new("Power BI", "Power BI", "Tool", "\u0110\u01b0a dashboard v\u00e0 tr\u1ef1c quan h\u00f3a d\u1eef li\u1ec7u v\u00e0o ph\u1ea7n Projects."),
            new("Marketing", "Marketing", "Course", "M\u00f4 t\u1ea3 vi\u1ec7c l\u1eadp k\u1ebf ho\u1ea1ch chi\u1ebfn d\u1ecbch v\u00e0 ph\u00e2n t\u00edch kh\u00e1ch h\u00e0ng m\u1ee5c ti\u00eau."),
            new("SEO", "SEO", "Project", "N\u00eau r\u00f5 nghi\u00ean c\u1ee9u t\u1eeb kh\u00f3a, t\u1ed1i \u01b0u n\u1ed9i dung v\u00e0 \u0111o l\u01b0\u1eddng traffic."),
            new("Content", "Content Writing", "Project", "M\u00f4 t\u1ea3 vi\u1ec7c vi\u1ebft n\u1ed9i dung, l\u00ean l\u1ecbch \u0111\u0103ng b\u00e0i v\u00e0 theo d\u00f5i hi\u1ec7u qu\u1ea3."),
            new("Figma", "Figma", "Tool", "\u0110\u01b0a prototype, wireframe v\u00e0 design system v\u00e0o portfolio."),
            new("UI/UX", "User Research", "Course", "M\u00f4 t\u1ea3 ph\u1ecfng v\u1ea5n ng\u01b0\u1eddi d\u00f9ng, persona v\u00e0 usability testing."),
            new("Ke toan", "Accounting", "Course", "G\u1eafn m\u00f4n h\u1ecdc v\u1edbi l\u1eadp b\u00e1o c\u00e1o, \u0111\u1ecbnh kho\u1ea3n v\u00e0 ki\u1ec3m tra s\u1ed1 li\u1ec7u."),
            new("Tai chinh", "Financial Analysis", "Course", "M\u00f4 t\u1ea3 ph\u00e2n t\u00edch chi ph\u00ed, doanh thu v\u00e0 ch\u1ec9 s\u1ed1 t\u00e0i ch\u00ednh."),
            new("Nhan su", "HR Operations", "Course", "M\u00f4 t\u1ea3 quy tr\u00ecnh tuy\u1ec3n d\u1ee5ng, s\u00e0ng l\u1ecdc CV v\u00e0 onboarding."),
            new("Thuyet trinh", "Communication", "Activity", "\u0110\u01b0a k\u1ef9 n\u0103ng tr\u00ecnh b\u00e0y v\u00e0 giao ti\u1ebfp v\u00e0o Activities."),
            new("Lam viec nhom", "Teamwork", "Activity", "\u0110\u01b0a k\u1ef9 n\u0103ng ph\u1ed1i h\u1ee3p nh\u00f3m v\u00e0 qu\u1ea3n l\u00fd c\u00f4ng vi\u1ec7c v\u00e0o CV.")
        };

        var roles = new List<CareerRole>
        {
            new("Backend Developer Intern", ["C#", "SQL Server", "REST API", "Git", "OOP", "Web Development"], "Student backend template with Technical Skills and Projects before Education."),
            new("Data Analyst Intern", ["SQL Server", "Excel", "Python", "Database Design"], "Analyst template with Projects, Data Skills and Education."),
            new("Web Developer Intern", ["Web Development", "HTML", "CSS", "Git", "Authentication"], "Web template with project screenshots optional, but ATS export should stay single-column."),
            new("QA Tester Intern", ["Testing", "Git", "Web Development", "Database Design"], "QA template with test cases, bug reports and documentation evidence."),
            new("Business Analyst Intern", ["Database Design", "Excel", "Web Development", "Communication"], "BA template with domain understanding, documentation and communication evidence."),
            new("Marketing Intern", ["Marketing", "SEO", "Content Writing", "Excel", "Communication"], "Marketing template with campaign, content and KPI evidence."),
            new("UI/UX Designer Intern", ["Figma", "User Research", "Communication", "Web Development"], "Design template with portfolio, case study and research evidence."),
            new("Finance Analyst Intern", ["Accounting", "Financial Analysis", "Excel", "Data Analysis"], "Finance template with analysis, reporting and Excel evidence."),
            new("HR Intern", ["HR Operations", "Communication", "Teamwork", "Excel"], "HR template with recruitment process, screening and coordination evidence.")
        };

        var samples = new List<ResumeGenerationRequest>
        {
            new(
                new StudentProfile(
                    "Nguy\u1ec5n V\u0103n A",
                    "nguyenvana@example.com",
                    "C\u00f4ng ngh\u1ec7 th\u00f4ng tin",
                    "Student",
                    ["C\u01a1 s\u1edf d\u1eef li\u1ec7u", "L\u1eadp tr\u00ecnh h\u01b0\u1edbng \u0111\u1ed1i t\u01b0\u1ee3ng", "L\u1eadp tr\u00ecnh web"],
                    ["C#", "SQL Server", "HTML", "CSS", "Git c\u01a1 b\u1ea3n"],
                    ["Website b\u00e1n h\u00e0ng c\u00f3 \u0111\u0103ng nh\u1eadp, gi\u1ecf h\u00e0ng, qu\u1ea3n l\u00fd s\u1ea3n ph\u1ea9m"],
                    ["L\u00e0m vi\u1ec7c nh\u00f3m trong \u0111\u1ed3 \u00e1n m\u00f4n h\u1ecdc"],
                    "\u1ee8ng tuy\u1ec3n Backend Developer Intern"),
                "Backend Developer Intern",
                "Y\u00eau c\u1ea7u C# ho\u1eb7c Java, SQL database, REST API, Git, OOP. C\u00f3 project web l\u00e0 l\u1ee3i th\u1ebf.",
                "English",
                true),
            new(
                new StudentProfile(
                    "Tr\u1ea7n Th\u1ecb B",
                    "tranthib@example.com",
                    "H\u1ec7 th\u1ed1ng th\u00f4ng tin qu\u1ea3n l\u00fd",
                    "Student",
                    ["C\u01a1 s\u1edf d\u1eef li\u1ec7u", "Ph\u00e2n t\u00edch d\u1eef li\u1ec7u", "X\u00e1c su\u1ea5t th\u1ed1ng k\u00ea"],
                    ["SQL Server", "Excel", "Power BI", "Python c\u01a1 b\u1ea3n"],
                    ["Dashboard ph\u00e2n t\u00edch doanh thu b\u1eb1ng Excel v\u00e0 Power BI"],
                    ["Thuy\u1ebft tr\u00ecnh b\u00e1o c\u00e1o d\u1eef li\u1ec7u cu\u1ed1i k\u1ef3"],
                    "\u1ee8ng tuy\u1ec3n Data Analyst Intern"),
                "Data Analyst Intern",
                "C\u1ea7n SQL, Excel, Power BI, Python, data cleaning, dashboard v\u00e0 kh\u1ea3 n\u0103ng tr\u00ecnh b\u00e0y insight.",
                "English",
                true),
            new(
                new StudentProfile(
                    "L\u00ea Minh C",
                    "leminhc@example.com",
                    "Marketing",
                    "Student",
                    ["Marketing c\u0103n b\u1ea3n", "Nghi\u00ean c\u1ee9u th\u1ecb tr\u01b0\u1eddng", "Truy\u1ec1n th\u00f4ng s\u1ed1"],
                    ["Marketing", "SEO", "Content Writing", "Excel"],
                    ["L\u1eadp k\u1ebf ho\u1ea1ch chi\u1ebfn d\u1ecbch social media v\u00e0 vi\u1ebft content cho s\u1ea3n ph\u1ea9m m\u1edbi"],
                    ["Thuy\u1ebft tr\u00ecnh k\u1ebf ho\u1ea1ch marketing nh\u00f3m"],
                    "\u1ee8ng tuy\u1ec3n Marketing Intern"),
                "Marketing Intern",
                "C\u1ea7n digital marketing, SEO, content writing, Excel, giao ti\u1ebfp v\u00e0 theo d\u00f5i KPI chi\u1ebfn d\u1ecbch.",
                "English",
                true),
            new(
                new StudentProfile(
                    "Ph\u1ea1m Ng\u1ecdc D",
                    "phamngocd@example.com",
                    "Thi\u1ebft k\u1ebf \u0111\u1ed3 h\u1ecda",
                    "Student",
                    ["UI/UX Design", "H\u00e0nh vi ng\u01b0\u1eddi d\u00f9ng", "Thi\u1ebft k\u1ebf giao di\u1ec7n"],
                    ["Figma", "User Research", "HTML", "CSS"],
                    ["Thi\u1ebft k\u1ebf prototype \u1ee9ng d\u1ee5ng \u0111\u1eb7t l\u1ecbch kh\u00e1m b\u1eb1ng Figma"],
                    ["Ph\u1ecfng v\u1ea5n ng\u01b0\u1eddi d\u00f9ng v\u00e0 thuy\u1ebft tr\u00ecnh case study"],
                    "\u1ee8ng tuy\u1ec3n UI/UX Designer Intern"),
                "UI/UX Designer Intern",
                "C\u1ea7n Figma, user research, wireframe, prototype, communication v\u00e0 portfolio case study.",
                "English",
                true),
            new(
                new StudentProfile(
                    "\u0110\u1ed7 Quang E",
                    "doquange@example.com",
                    "K\u1ebf to\u00e1n - T\u00e0i ch\u00ednh",
                    "Student",
                    ["K\u1ebf to\u00e1n t\u00e0i ch\u00ednh", "Ph\u00e2n t\u00edch t\u00e0i ch\u00ednh", "Excel \u1ee9ng d\u1ee5ng"],
                    ["Accounting", "Financial Analysis", "Excel", "Data Analysis"],
                    ["B\u1ea3ng ph\u00e2n t\u00edch chi ph\u00ed v\u00e0 doanh thu cho doanh nghi\u1ec7p gia \u0111\u00ecnh"],
                    ["L\u00e0m vi\u1ec7c nh\u00f3m trong b\u00e0i t\u1eadp t\u00ecnh hu\u1ed1ng t\u00e0i ch\u00ednh"],
                    "\u1ee8ng tuy\u1ec3n Finance Analyst Intern"),
                "Finance Analyst Intern",
                "C\u1ea7n accounting, financial analysis, Excel, data analysis v\u00e0 b\u00e1o c\u00e1o s\u1ed1 li\u1ec7u ch\u00ednh x\u00e1c.",
                "English",
                true),
            new(
                new StudentProfile(
                    "Ho\u00e0ng Lan F",
                    "hoanglanf@example.com",
                    "Qu\u1ea3n tr\u1ecb nh\u00e2n l\u1ef1c",
                    "Student",
                    ["Qu\u1ea3n tr\u1ecb nh\u00e2n s\u1ef1", "Tuy\u1ec3n d\u1ee5ng", "H\u00e0nh vi t\u1ed5 ch\u1ee9c"],
                    ["HR Operations", "Communication", "Teamwork", "Excel"],
                    ["M\u00f4 ph\u1ecfng quy tr\u00ecnh s\u00e0ng l\u1ecdc CV v\u00e0 l\u1eadp k\u1ebf ho\u1ea1ch onboarding nh\u00e2n vi\u00ean m\u1edbi"],
                    ["\u0110i\u1ec1u ph\u1ed1i nh\u00f3m trong s\u1ef1 ki\u1ec7n c\u00e2u l\u1ea1c b\u1ed9"],
                    "\u1ee8ng tuy\u1ec3n HR Intern"),
                "HR Intern",
                "C\u1ea7n recruitment process, CV screening, communication, teamwork, Excel v\u00e0 h\u1ed7 tr\u1ee3 onboarding.",
                "English",
                true)
        };

        return new DemoDataset(taxonomy, academicRules, roles, samples);
    }
}

record DemoDataset(
    List<SkillTaxonomyItem> SkillTaxonomy,
    List<AcademicRule> AcademicRules,
    List<CareerRole> CareerRoles,
    List<ResumeGenerationRequest> Samples)
{
    public ResumeGenerationRequest SampleRequest => Samples[0];
}

record SkillTaxonomyItem(string Name, string Category, List<string> Aliases);
record AcademicRule(string Trigger, string Skill, string EvidenceType, string ResumeSuggestion);
record CareerRole(string Name, List<string> RequiredSkills, string TemplateHint);

record StudentProfile(
    string FullName,
    string Email,
    string Major,
    string ExperienceLevel,
    List<string> Courses,
    List<string> Skills,
    List<string> Projects,
    List<string> Activities,
    string CareerGoal);

record JobAnalyzeRequest(string JobTitle, string JobDescription);
record JobAnalysis(string JobTitle, string Seniority, List<string> RequiredSkills, List<string> Keywords, string OriginalText);

record AcademicCareerRequest(string Major, List<string> Courses, List<string> Projects, List<string> Activities);
record SkillEvidence(string Source, string Skill, string EvidenceType, string ResumeSuggestion);
record AcademicRoleMatch(string RoleName, decimal MatchScore, List<string> MatchedSkills, List<string> MissingSkills);
record AcademicCareerResult(string Major, List<string> InferredSkills, List<SkillEvidence> Evidence, List<AcademicRoleMatch> RoleMatches);

record TemplateRequest(string TargetJobTitle, string ExperienceLevel, bool AtsFriendly, string Language);
record ResumeTemplate(string TargetJobTitle, string TemplateName, string Language, bool AtsFriendly, List<string> Sections, List<string> FormatWarnings);

record ResumeGenerationRequest(StudentProfile Profile, string TargetJobTitle, string JobDescription, string Language, bool PreferOnePage);
record ResumeDraft(
    string CandidateName,
    string TargetJobTitle,
    string Summary,
    List<string> Sections,
    List<string> Skills,
    List<string> ProjectBullets,
    List<string> MatchedJobSkills,
    List<string> MissingJobSkills,
    List<string> HumanReviewNotes);

record AtsRequest(string JobTitle, string JobDescription, ResumeDraft Resume);
record AtsResult(
    decimal OverallScore,
    decimal ParseabilityScore,
    decimal KeywordMatchScore,
    decimal SkillCoverageScore,
    decimal FormatSafetyScore,
    List<string> MatchedSkills,
    List<string> MissingSkills,
    List<string> Advice);

record CareerRecommendationRequest(StudentProfile Profile);
record CareerRecommendation(string RoleName, decimal MatchScore, List<string> StrongSkills, List<string> MissingSkills, string TemplateHint, List<string> LearningPath);
record CareerRecommendationResult(string CandidateName, List<CareerRecommendation> Recommendations);
record PersonalizedResumeResult(ResumeDraft Resume, AtsResult Ats, List<string> PersonalizationActions, string Explanation);
record PipelineResult(JobAnalysis JobAnalysis, AcademicCareerResult AcademicCareerMapping, ResumeTemplate Template, ResumeDraft Resume, AtsResult AtsOptimization, CareerRecommendationResult CareerRecommendations);
