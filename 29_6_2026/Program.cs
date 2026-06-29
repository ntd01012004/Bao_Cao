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
        Title = "Student Skill Intelligence Demo API",
        Version = "v1",
        Description = "Demo Student Academic Data Modeling, Skill Mapping, Competency Profile Generation and Skill Inference."
    });
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Student Skill Intelligence Demo API v1");
    options.RoutePrefix = "swagger";
});

const decimal MasteryThreshold = 0.70m;
const decimal EmergingThreshold = 0.55m;

var data = DemoData.Create();

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "Healthy",
    app = "Student Skill Intelligence Demo",
    generatedAt = DateTimeOffset.Now
}))
.WithTags("System")
.WithSummary("Check API health");

app.MapGet("/api/students", () => Results.Ok(data.Students))
    .WithTags("Student Academic Data Modeling")
    .WithSummary("Get student academic records");

app.MapGet("/api/students/{studentId}/academic-data", (string studentId) =>
{
    var student = data.Students.FirstOrDefault(item => item.Id.Equals(studentId, StringComparison.OrdinalIgnoreCase));
    if (student is null)
    {
        return Results.NotFound(new { message = $"Student '{studentId}' was not found." });
    }

    return Results.Ok(new
    {
        student,
        enrollments = data.Enrollments.Where(item => item.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase)),
        assessments = data.Assessments.Where(item => item.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase)),
        activities = data.Activities.Where(item => item.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase))
    });
})
.WithTags("Student Academic Data Modeling")
.WithSummary("Get detailed academic data for one student");

app.MapGet("/api/courses", () => Results.Ok(data.Courses))
    .WithTags("Student Academic Data Modeling")
    .WithSummary("Get courses");

app.MapGet("/api/skills", () => Results.Ok(data.Skills))
    .WithTags("Skill Mapping")
    .WithSummary("Get skill catalog");

app.MapGet("/api/skill-map", () =>
{
    var result = data.SkillMappings.Select(mapping => new SkillMapItem(
        mapping.CourseCode,
        data.Courses.First(course => course.Code == mapping.CourseCode).Name,
        mapping.AssessmentType,
        mapping.SkillCode,
        data.Skills.First(skill => skill.Code == mapping.SkillCode).Name,
        mapping.EvidenceWeight,
        mapping.Relationship));

    return Results.Ok(result);
})
.WithTags("Skill Mapping")
.WithSummary("Get mapping from courses and assessment types to skills");

app.MapGet("/api/competency-profiles", () =>
{
    var profiles = data.Students.Select(student => GenerateProfile(student, data, MasteryThreshold, EmergingThreshold));
    return Results.Ok(profiles);
})
.WithTags("Competency Profile Generation")
.WithSummary("Generate competency profiles for all students");

app.MapGet("/api/competency-profiles/{studentId}", (string studentId) =>
{
    var student = data.Students.FirstOrDefault(item => item.Id.Equals(studentId, StringComparison.OrdinalIgnoreCase));
    if (student is null)
    {
        return Results.NotFound(new { message = $"Student '{studentId}' was not found." });
    }

    return Results.Ok(GenerateProfile(student, data, MasteryThreshold, EmergingThreshold));
})
.WithTags("Competency Profile Generation")
.WithSummary("Generate a competency profile for one student");

app.MapGet("/api/skill-inference/{studentId}", (string studentId) =>
{
    var student = data.Students.FirstOrDefault(item => item.Id.Equals(studentId, StringComparison.OrdinalIgnoreCase));
    if (student is null)
    {
        return Results.NotFound(new { message = $"Student '{studentId}' was not found." });
    }

    var profile = GenerateProfile(student, data, MasteryThreshold, EmergingThreshold);
    return Results.Ok(new
    {
        profile.Student,
        profile.OverallSkillMastery,
        inferredSkills = profile.Skills,
        inferenceRules = new[]
        {
            "Direct evidence: assessment score is weighted through the skill map.",
            "Learning evidence: practice minutes and completed activities add a small confidence signal.",
            "Prerequisite inference: mastery of prerequisite skills contributes 15% to dependent skills.",
            "Confidence is based on number of evidence sources and recent activity."
        }
    });
})
.WithTags("Skill Inference")
.WithSummary("Infer student skills from assessments, activities and prerequisite relationships");

app.MapPost("/api/skill-inference/evaluate", (SkillEvidenceRequest request) =>
{
    if (request.Score < 0 || request.MaxScore <= 0 || request.Score > request.MaxScore)
    {
        return Results.BadRequest(new { message = "Score must be between 0 and MaxScore, and MaxScore must be greater than 0." });
    }

    var student = data.Students.FirstOrDefault(item => item.Id.Equals(request.StudentId, StringComparison.OrdinalIgnoreCase));
    var course = data.Courses.FirstOrDefault(item => item.Code.Equals(request.CourseCode, StringComparison.OrdinalIgnoreCase));
    if (student is null)
    {
        return Results.BadRequest(new { message = $"Unknown student '{request.StudentId}'." });
    }

    if (course is null)
    {
        return Results.BadRequest(new { message = $"Unknown course '{request.CourseCode}'." });
    }

    var relatedMappings = data.SkillMappings
        .Where(mapping =>
            mapping.CourseCode.Equals(request.CourseCode, StringComparison.OrdinalIgnoreCase) &&
            mapping.AssessmentType.Equals(request.AssessmentType, StringComparison.OrdinalIgnoreCase))
        .ToList();

    if (relatedMappings.Count == 0)
    {
        return Results.BadRequest(new { message = $"No skill mapping found for {request.CourseCode} / {request.AssessmentType}." });
    }

    var normalizedScore = request.Score / request.MaxScore;
    var inferredSkills = relatedMappings.Select(mapping =>
    {
        var skill = data.Skills.First(item => item.Code == mapping.SkillCode);
        var inferredMastery = decimal.Round(normalizedScore * mapping.EvidenceWeight, 4);
        return new
        {
            skill.Code,
            skill.Name,
            evidenceContribution = inferredMastery,
            predictedLevel = ClassifySkill(inferredMastery, MasteryThreshold, EmergingThreshold)
        };
    });

    return Results.Ok(new
    {
        student.Id,
        student.FullName,
        course.Code,
        course.Name,
        request.AssessmentType,
        normalizedScore = decimal.Round(normalizedScore, 4),
        inferredSkills
    });
})
.WithTags("Skill Inference")
.WithSummary("Infer skills from a new assessment evidence item");

app.MapGet("/api/analytics/cohort", () =>
{
    var profiles = data.Students.Select(student => GenerateProfile(student, data, MasteryThreshold, EmergingThreshold)).ToList();

    return Results.Ok(new
    {
        studentCount = profiles.Count,
        averageSkillMastery = decimal.Round(profiles.Average(profile => profile.OverallSkillMastery), 4),
        profileDistribution = profiles
            .GroupBy(profile => profile.ProfileLevel)
            .Select(group => new { profileLevel = group.Key, count = group.Count() })
            .OrderBy(item => item.profileLevel),
        skillAverages = data.Skills.Select(skill =>
        {
            var average = profiles
                .SelectMany(profile => profile.Skills)
                .Where(result => result.SkillCode == skill.Code)
                .Average(result => result.Mastery);

            return new
            {
                skill.Code,
                skill.Name,
                averageMastery = decimal.Round(average, 4),
                status = ClassifySkill(average, MasteryThreshold, EmergingThreshold)
            };
        })
    });
})
.WithTags("Analytics")
.WithSummary("Get cohort analytics from generated competency profiles");

app.MapGet("/api/analytics/skill-gaps", () =>
{
    var profiles = data.Students.Select(student => GenerateProfile(student, data, MasteryThreshold, EmergingThreshold)).ToList();

    var gaps = data.Skills.Select(skill =>
    {
        var skillResults = profiles
            .SelectMany(profile => profile.Skills)
            .Where(result => result.SkillCode == skill.Code)
            .ToList();

        var average = skillResults.Average(result => result.Mastery);
        var weakCount = skillResults.Count(result => result.Mastery < MasteryThreshold);

        return new
        {
            skill.Code,
            skill.Name,
            averageMastery = decimal.Round(average, 4),
            weakStudentCount = weakCount,
            gapScore = decimal.Round((1 - average) + weakCount * 0.08m, 4)
        };
    })
    .OrderByDescending(item => item.gapScore);

    return Results.Ok(gaps);
})
.WithTags("Analytics")
.WithSummary("Find cohort-wide skill gaps");

app.Run();

static CompetencyProfile GenerateProfile(
    Student student,
    DemoData data,
    decimal masteryThreshold,
    decimal emergingThreshold)
{
    var assessments = data.Assessments
        .Where(item => item.StudentId.Equals(student.Id, StringComparison.OrdinalIgnoreCase))
        .ToList();

    var activities = data.Activities
        .Where(item => item.StudentId.Equals(student.Id, StringComparison.OrdinalIgnoreCase))
        .ToList();

    var skillResults = data.Skills.Select(skill =>
    {
        var directEvidence = new List<decimal>();
        var confidenceSignals = 0;

        foreach (var assessment in assessments)
        {
            var mappings = data.SkillMappings.Where(mapping =>
                mapping.CourseCode == assessment.CourseCode &&
                mapping.AssessmentType == assessment.AssessmentType &&
                mapping.SkillCode == skill.Code);

            foreach (var mapping in mappings)
            {
                directEvidence.Add((assessment.Score / assessment.MaxScore) * mapping.EvidenceWeight);
                confidenceSignals++;
            }
        }

        var directMastery = directEvidence.Count == 0 ? 0 : directEvidence.Average();
        var activityMinutes = activities
            .Where(activity => activity.SkillCode == skill.Code)
            .Sum(activity => activity.DurationMinutes);

        var completedActivityCount = activities
            .Count(activity => activity.SkillCode == skill.Code && activity.Completed);

        var activityBoost = Math.Min(0.10m, (activityMinutes / 600m) + (completedActivityCount * 0.02m));
        var prerequisiteBoost = CalculatePrerequisiteBoost(skill, data.Skills, directEvidence, masteryThreshold);
        var mastery = Math.Min(1, directMastery + activityBoost + prerequisiteBoost);
        var confidence = Math.Min(1, (confidenceSignals * 0.25m) + (completedActivityCount * 0.10m) + (activityMinutes > 0 ? 0.15m : 0));

        return new SkillInferenceResult(
            skill.Code,
            skill.Name,
            decimal.Round(mastery, 4),
            ClassifySkill(mastery, masteryThreshold, emergingThreshold),
            decimal.Round(confidence, 4),
            directEvidence.Count,
            activityMinutes);
    }).ToList();

    var overall = skillResults.Count == 0 ? 0 : skillResults.Average(item => item.Mastery);
    var profileLevel = ClassifyProfile(overall, skillResults.Count(item => item.Status == SkillStatus.Mastered));
    var recommendations = RecommendNextSkills(skillResults, data.Resources);

    return new CompetencyProfile(
        student,
        decimal.Round(overall, 4),
        profileLevel,
        skillResults,
        recommendations);
}

static decimal CalculatePrerequisiteBoost(
    Skill skill,
    IReadOnlyCollection<Skill> skills,
    IReadOnlyCollection<decimal> directEvidence,
    decimal masteryThreshold)
{
    if (skill.PrerequisiteSkillCodes.Count == 0 || directEvidence.Count == 0)
    {
        return 0;
    }

    var directMastery = directEvidence.Average();
    return directMastery >= masteryThreshold ? 0.05m : 0.02m;
}

static SkillStatus ClassifySkill(decimal mastery, decimal masteryThreshold, decimal emergingThreshold)
{
    if (mastery >= masteryThreshold)
    {
        return SkillStatus.Mastered;
    }

    if (mastery >= emergingThreshold)
    {
        return SkillStatus.Emerging;
    }

    return SkillStatus.NeedsSupport;
}

static ProfileLevel ClassifyProfile(decimal overallMastery, int masteredSkillCount)
{
    if (overallMastery >= 0.78m && masteredSkillCount >= 5)
    {
        return ProfileLevel.Advanced;
    }

    if (overallMastery >= 0.65m && masteredSkillCount >= 3)
    {
        return ProfileLevel.Proficient;
    }

    if (overallMastery >= 0.50m)
    {
        return ProfileLevel.Developing;
    }

    return ProfileLevel.AtRisk;
}

static IReadOnlyCollection<LearningResource> RecommendNextSkills(
    IReadOnlyCollection<SkillInferenceResult> skillResults,
    IReadOnlyCollection<LearningResource> resources)
{
    var weakSkillCodes = skillResults
        .Where(item => item.Status != SkillStatus.Mastered)
        .OrderBy(item => item.Mastery)
        .Take(3)
        .Select(item => item.SkillCode)
        .ToHashSet();

    return resources.Where(resource => weakSkillCodes.Contains(resource.SkillCode)).ToList();
}

record DemoData(
    IReadOnlyCollection<Student> Students,
    IReadOnlyCollection<Course> Courses,
    IReadOnlyCollection<Enrollment> Enrollments,
    IReadOnlyCollection<AssessmentRecord> Assessments,
    IReadOnlyCollection<LearningActivity> Activities,
    IReadOnlyCollection<Skill> Skills,
    IReadOnlyCollection<SkillMapping> SkillMappings,
    IReadOnlyCollection<LearningResource> Resources)
{
    public static DemoData Create()
    {
        var students = new List<Student>
        {
            new("S001", "An Nguyen", "SE-K18", 3.42m, 92),
            new("S002", "Binh Tran", "SE-K18", 2.74m, 78),
            new("S003", "Chi Le", "SE-K19", 3.12m, 85),
            new("S004", "Dung Pham", "SE-K19", 3.78m, 96)
        };

        var courses = new List<Course>
        {
            new("SE101", "Nhập môn lập trình", 3),
            new("SE204", "Hệ cơ sở dữ liệu", 3),
            new("SE305", "Dự án kỹ thuật phần mềm", 4),
            new("SE410", "Học máy ứng dụng", 3)
        };

        var enrollments = new List<Enrollment>
        {
            new("S001", "SE101", "A"), new("S001", "SE204", "B+"), new("S001", "SE305", "A-"), new("S001", "SE410", "B+"),
            new("S002", "SE101", "C+"), new("S002", "SE204", "B-"), new("S002", "SE305", "C+"), new("S002", "SE410", "C"),
            new("S003", "SE101", "B+"), new("S003", "SE204", "B"), new("S003", "SE305", "B+"), new("S003", "SE410", "B"),
            new("S004", "SE101", "A"), new("S004", "SE204", "A"), new("S004", "SE305", "A"), new("S004", "SE410", "A-")
        };

        var skills = new List<Skill>
        {
            new("SK-PROG", "Tư duy lập trình", "Viết chương trình đúng bằng cấu trúc điều khiển, hàm và cấu trúc dữ liệu.", Array.Empty<string>()),
            new("SK-DB", "Mô hình hóa dữ liệu", "Thiết kế lược đồ quan hệ và phân tích tính toàn vẹn dữ liệu.", new[] { "SK-PROG" }),
            new("SK-SQL", "Truy vấn SQL", "Viết truy vấn SQL để truy xuất, tổng hợp và kiểm tra chất lượng dữ liệu.", new[] { "SK-DB" }),
            new("SK-REQ", "Phân tích yêu cầu", "Chuyển nhu cầu của bên liên quan thành yêu cầu phần mềm rõ ràng.", Array.Empty<string>()),
            new("SK-TEAM", "Hợp tác nhóm", "Lập kế hoạch công việc, trao đổi tiến độ và xử lý vấn đề trong nhóm.", Array.Empty<string>()),
            new("SK-ML", "Đánh giá mô hình", "Đánh giá mô hình dự đoán bằng chỉ số và chiến lược kiểm định phù hợp.", new[] { "SK-PROG" }),
            new("SK-COMM", "Giao tiếp kỹ thuật", "Trình bày quyết định kỹ thuật và bằng chứng cho nhiều nhóm người nghe.", Array.Empty<string>())
        };

        var mappings = new List<SkillMapping>
        {
            new("SE101", "Quiz", "SK-PROG", 0.85m, "Direct"),
            new("SE101", "Lab", "SK-PROG", 0.95m, "Direct"),
            new("SE204", "DesignAssignment", "SK-DB", 0.90m, "Direct"),
            new("SE204", "Lab", "SK-SQL", 0.90m, "Direct"),
            new("SE305", "RequirementBrief", "SK-REQ", 0.85m, "Direct"),
            new("SE305", "SprintReview", "SK-TEAM", 0.75m, "Direct"),
            new("SE305", "Presentation", "SK-COMM", 0.85m, "Direct"),
            new("SE410", "ModelReport", "SK-ML", 0.90m, "Direct"),
            new("SE410", "Presentation", "SK-COMM", 0.70m, "Supporting")
        };

        var assessments = new List<AssessmentRecord>
        {
            new("S001", "SE101", "Quiz", 8.2m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-34))),
            new("S001", "SE101", "Lab", 8.5m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-31))),
            new("S001", "SE204", "DesignAssignment", 7.8m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-22))),
            new("S001", "SE204", "Lab", 7.2m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-18))),
            new("S001", "SE305", "RequirementBrief", 7.7m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-13))),
            new("S001", "SE305", "SprintReview", 8.0m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-9))),
            new("S001", "SE305", "Presentation", 7.5m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-6))),
            new("S001", "SE410", "ModelReport", 7.6m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-3))),
            new("S002", "SE101", "Quiz", 6.1m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-34))),
            new("S002", "SE101", "Lab", 5.8m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-31))),
            new("S002", "SE204", "DesignAssignment", 6.2m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-22))),
            new("S002", "SE204", "Lab", 5.9m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-18))),
            new("S002", "SE305", "RequirementBrief", 6.4m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-13))),
            new("S002", "SE305", "SprintReview", 5.7m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-9))),
            new("S002", "SE305", "Presentation", 6.0m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-6))),
            new("S002", "SE410", "ModelReport", 5.6m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-3))),
            new("S003", "SE101", "Quiz", 7.4m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-34))),
            new("S003", "SE101", "Lab", 7.8m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-31))),
            new("S003", "SE204", "DesignAssignment", 7.2m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-22))),
            new("S003", "SE204", "Lab", 6.8m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-18))),
            new("S003", "SE305", "RequirementBrief", 7.0m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-13))),
            new("S003", "SE305", "SprintReview", 7.5m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-9))),
            new("S003", "SE305", "Presentation", 8.0m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-6))),
            new("S003", "SE410", "ModelReport", 7.1m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-3))),
            new("S004", "SE101", "Quiz", 9.2m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-34))),
            new("S004", "SE101", "Lab", 9.0m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-31))),
            new("S004", "SE204", "DesignAssignment", 8.9m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-22))),
            new("S004", "SE204", "Lab", 8.7m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-18))),
            new("S004", "SE305", "RequirementBrief", 8.8m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-13))),
            new("S004", "SE305", "SprintReview", 9.0m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-9))),
            new("S004", "SE305", "Presentation", 9.2m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-6))),
            new("S004", "SE410", "ModelReport", 8.5m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-3)))
        };

        var activities = new List<LearningActivity>
        {
            new("S001", "SK-PROG", "Bài luyện lập trình", 65, true), new("S001", "SK-SQL", "Bài luyện SQL", 45, true), new("S001", "SK-ML", "Sổ tay kiểm định mô hình", 50, true),
            new("S002", "SK-PROG", "Bài luyện lập trình", 25, true), new("S002", "SK-SQL", "Bài luyện SQL", 20, false), new("S002", "SK-TEAM", "Phản tư sprint", 0, false),
            new("S003", "SK-PROG", "Bài luyện lập trình", 45, true), new("S003", "SK-COMM", "Luyện thuyết trình", 40, true), new("S003", "SK-ML", "Sổ tay kiểm định mô hình", 35, true),
            new("S004", "SK-PROG", "Bài luyện lập trình", 80, true), new("S004", "SK-SQL", "Bài luyện SQL", 65, true), new("S004", "SK-COMM", "Luyện thuyết trình", 45, true)
        };

        var resources = new List<LearningResource>
        {
            new("R01", "Bài luyện tư duy lập trình", "SK-PROG"),
            new("R02", "Workshop chuẩn hóa ERD", "SK-DB"),
            new("R03", "Lab truy vấn tổng hợp SQL", "SK-SQL"),
            new("R04", "Mẫu phỏng vấn yêu cầu", "SK-REQ"),
            new("R05", "Checklist hợp tác trong sprint", "SK-TEAM"),
            new("R06", "Notebook đánh giá mô hình", "SK-ML"),
            new("R07", "Rubric thuyết trình kỹ thuật", "SK-COMM")
        };

        return new DemoData(students, courses, enrollments, assessments, activities, skills, mappings, resources);
    }
}

record Student(string Id, string FullName, string Cohort, decimal Gpa, int AttendanceRate);

record Course(string Code, string Name, int Credits);

record Enrollment(string StudentId, string CourseCode, string Grade);

record AssessmentRecord(string StudentId, string CourseCode, string AssessmentType, decimal Score, decimal MaxScore, DateOnly AssessedOn);

record LearningActivity(string StudentId, string SkillCode, string ActivityName, int DurationMinutes, bool Completed);

record Skill(string Code, string Name, string Description, IReadOnlyCollection<string> PrerequisiteSkillCodes);

record SkillMapping(string CourseCode, string AssessmentType, string SkillCode, decimal EvidenceWeight, string Relationship);

record SkillMapItem(
    string CourseCode,
    string CourseName,
    string AssessmentType,
    string SkillCode,
    string SkillName,
    decimal EvidenceWeight,
    string Relationship);

record LearningResource(string Id, string Title, string SkillCode);

record SkillInferenceResult(
    string SkillCode,
    string SkillName,
    decimal Mastery,
    SkillStatus Status,
    decimal Confidence,
    int EvidenceCount,
    int ActivityMinutes);

record CompetencyProfile(
    Student Student,
    decimal OverallSkillMastery,
    ProfileLevel ProfileLevel,
    IReadOnlyCollection<SkillInferenceResult> Skills,
    IReadOnlyCollection<LearningResource> RecommendedResources);

record SkillEvidenceRequest(string StudentId, string CourseCode, string AssessmentType, decimal Score, decimal MaxScore);

enum SkillStatus
{
    NeedsSupport,
    Emerging,
    Mastered
}

enum ProfileLevel
{
    AtRisk,
    Developing,
    Proficient,
    Advanced
}
