var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var program = new AcademicProgram(
    "SE",
    "Software Engineering",
    "Outcome-Based Education demo program with PLO, CLO and competency mapping.");

var plos = new List<LearningOutcome>
{
    new("PLO1", "Software Engineering Knowledge", "Apply computing, mathematics and software engineering knowledge to solve practical problems."),
    new("PLO2", "System Design", "Design, implement and evaluate software systems that satisfy user and business requirements."),
    new("PLO3", "Professional Practice", "Communicate, collaborate and act ethically in software engineering contexts.")
};

var courses = new List<Course>
{
    new("SE101", "Introduction to Software Engineering", 3),
    new("SE204", "Database Systems", 3),
    new("SE305", "Capstone Project", 4)
};

var clos = new List<CourseLearningOutcome>
{
    new("CLO1", "SE101", "Explain core software process models and engineering roles."),
    new("CLO2", "SE101", "Analyze simple stakeholder needs and convert them into requirements."),
    new("CLO3", "SE204", "Design normalized relational database schemas for business scenarios."),
    new("CLO4", "SE204", "Implement SQL queries and evaluate data quality constraints."),
    new("CLO5", "SE305", "Deliver a working software product using iterative team practices."),
    new("CLO6", "SE305", "Present technical decisions, risks and ethical considerations.")
};

var competencies = new List<Competency>
{
    new("COMP-REQ", "Requirements Analysis", "Elicit, document and validate software requirements."),
    new("COMP-DES", "Solution Design", "Create maintainable architecture, data and interface designs."),
    new("COMP-IMP", "Implementation", "Build, test and integrate working software components."),
    new("COMP-COM", "Communication", "Communicate technical ideas to technical and non-technical audiences."),
    new("COMP-ETH", "Ethics and Professionalism", "Apply responsible, ethical and professional judgment.")
};

var mappings = new List<OutcomeMapping>
{
    new("CLO1", "PLO1", "COMP-REQ", MasteryLevel.Introduced, 0.20m),
    new("CLO2", "PLO1", "COMP-REQ", MasteryLevel.Reinforced, 0.35m),
    new("CLO2", "PLO3", "COMP-COM", MasteryLevel.Introduced, 0.15m),
    new("CLO3", "PLO2", "COMP-DES", MasteryLevel.Reinforced, 0.40m),
    new("CLO4", "PLO1", "COMP-IMP", MasteryLevel.Reinforced, 0.30m),
    new("CLO4", "PLO2", "COMP-DES", MasteryLevel.Reinforced, 0.25m),
    new("CLO5", "PLO2", "COMP-IMP", MasteryLevel.Mastered, 0.50m),
    new("CLO5", "PLO3", "COMP-COM", MasteryLevel.Mastered, 0.25m),
    new("CLO6", "PLO3", "COMP-COM", MasteryLevel.Mastered, 0.25m),
    new("CLO6", "PLO3", "COMP-ETH", MasteryLevel.Reinforced, 0.20m)
};

var evidences = new List<StudentEvidence>
{
    new("S001", "SE101", "CLO1", "Quiz 1", 8.0m, 10.0m),
    new("S001", "SE101", "CLO2", "Requirement brief", 7.5m, 10.0m),
    new("S001", "SE204", "CLO3", "ERD assignment", 8.5m, 10.0m),
    new("S001", "SE204", "CLO4", "SQL lab", 7.0m, 10.0m),
    new("S001", "SE305", "CLO5", "Sprint demo", 8.0m, 10.0m),
    new("S001", "SE305", "CLO6", "Final presentation", 7.0m, 10.0m),
    new("S002", "SE101", "CLO1", "Quiz 1", 6.5m, 10.0m),
    new("S002", "SE101", "CLO2", "Requirement brief", 6.0m, 10.0m),
    new("S002", "SE204", "CLO3", "ERD assignment", 7.0m, 10.0m),
    new("S002", "SE204", "CLO4", "SQL lab", 6.5m, 10.0m),
    new("S002", "SE305", "CLO5", "Sprint demo", 7.5m, 10.0m),
    new("S002", "SE305", "CLO6", "Final presentation", 8.0m, 10.0m)
};

app.MapGet("/", () => Results.Ok(new
{
    name = "OBE Competency Framework Mini API",
    endpoints = new[]
    {
        "/api/program",
        "/api/courses",
        "/api/plos",
        "/api/clos",
        "/api/competencies",
        "/api/curriculum-map",
        "/api/attainment/summary",
        "/api/attainment/student/{studentId}"
    }
}));

app.MapGet("/api/program", () => Results.Ok(program));

app.MapGet("/api/courses", () => Results.Ok(courses));

app.MapGet("/api/plos", () => Results.Ok(plos));

app.MapGet("/api/clos", (string? courseCode) =>
{
    var result = string.IsNullOrWhiteSpace(courseCode)
        ? clos
        : clos.Where(clo => clo.CourseCode.Equals(courseCode, StringComparison.OrdinalIgnoreCase));

    return Results.Ok(result);
});

app.MapGet("/api/competencies", () => Results.Ok(competencies));

app.MapGet("/api/curriculum-map", () =>
{
    var result = mappings.Select(mapping => new
    {
        mapping.CloCode,
        Clo = clos.First(clo => clo.Code == mapping.CloCode).Description,
        mapping.PloCode,
        Plo = plos.First(plo => plo.Code == mapping.PloCode).Description,
        mapping.CompetencyCode,
        Competency = competencies.First(competency => competency.Code == mapping.CompetencyCode).Name,
        mapping.Level,
        mapping.Weight
    });

    return Results.Ok(result);
});

app.MapGet("/api/attainment/summary", () =>
{
    var cloAttainment = CalculateCloAttainment(evidences);
    var ploAttainment = CalculateMappedAttainment(cloAttainment, mappings, mapping => mapping.PloCode);
    var competencyAttainment = CalculateMappedAttainment(cloAttainment, mappings, mapping => mapping.CompetencyCode);

    return Results.Ok(new
    {
        threshold = 0.70m,
        cloAttainment,
        ploAttainment,
        competencyAttainment
    });
});

app.MapGet("/api/attainment/student/{studentId}", (string studentId) =>
{
    var studentEvidence = evidences
        .Where(evidence => evidence.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase))
        .ToList();

    if (studentEvidence.Count == 0)
    {
        return Results.NotFound(new { message = $"No evidence found for student '{studentId}'." });
    }

    var cloAttainment = CalculateCloAttainment(studentEvidence);
    var ploAttainment = CalculateMappedAttainment(cloAttainment, mappings, mapping => mapping.PloCode);
    var competencyAttainment = CalculateMappedAttainment(cloAttainment, mappings, mapping => mapping.CompetencyCode);

    return Results.Ok(new
    {
        studentId,
        threshold = 0.70m,
        cloAttainment,
        ploAttainment,
        competencyAttainment
    });
});

app.MapPost("/api/attainment/evaluate", (EvidenceRequest request) =>
{
    if (request.Score < 0 || request.MaxScore <= 0 || request.Score > request.MaxScore)
    {
        return Results.BadRequest(new { message = "Score must be between 0 and MaxScore, and MaxScore must be greater than 0." });
    }

    var clo = clos.FirstOrDefault(item => item.Code.Equals(request.CloCode, StringComparison.OrdinalIgnoreCase));
    if (clo is null)
    {
        return Results.BadRequest(new { message = $"Unknown CLO '{request.CloCode}'." });
    }

    var percentage = decimal.Round(request.Score / request.MaxScore, 4);
    var relatedMappings = mappings
        .Where(mapping => mapping.CloCode.Equals(request.CloCode, StringComparison.OrdinalIgnoreCase))
        .ToList();

    return Results.Ok(new
    {
        request.StudentId,
        request.CourseCode,
        request.CloCode,
        Clo = clo.Description,
        percentage,
        attained = percentage >= 0.70m,
        mappedOutcomes = relatedMappings
    });
});

app.Run();

static List<AttainmentResult> CalculateCloAttainment(IEnumerable<StudentEvidence> source)
{
    return source
        .GroupBy(evidence => evidence.CloCode)
        .Select(group =>
        {
            var percentage = group.Sum(evidence => evidence.Score) / group.Sum(evidence => evidence.MaxScore);

            return new AttainmentResult(
                group.Key,
                decimal.Round(percentage, 4),
                percentage >= 0.70m);
        })
        .OrderBy(result => result.Code)
        .ToList();
}

static List<AttainmentResult> CalculateMappedAttainment(
    IReadOnlyCollection<AttainmentResult> cloAttainment,
    IEnumerable<OutcomeMapping> mappings,
    Func<OutcomeMapping, string> selector)
{
    return mappings
        .GroupBy(selector)
        .Select(group =>
        {
            var weightedScore = 0m;
            var totalWeight = 0m;

            foreach (var mapping in group)
            {
                var attainment = cloAttainment.FirstOrDefault(item => item.Code == mapping.CloCode);
                if (attainment is null)
                {
                    continue;
                }

                weightedScore += attainment.Percentage * mapping.Weight;
                totalWeight += mapping.Weight;
            }

            var percentage = totalWeight == 0 ? 0 : weightedScore / totalWeight;

            return new AttainmentResult(
                group.Key,
                decimal.Round(percentage, 4),
                percentage >= 0.70m);
        })
        .OrderBy(result => result.Code)
        .ToList();
}

record AcademicProgram(string Code, string Name, string Description);

record Course(string Code, string Name, int Credits);

record LearningOutcome(string Code, string Name, string Description);

record CourseLearningOutcome(string Code, string CourseCode, string Description);

record Competency(string Code, string Name, string Description);

record OutcomeMapping(string CloCode, string PloCode, string CompetencyCode, MasteryLevel Level, decimal Weight);

record StudentEvidence(string StudentId, string CourseCode, string CloCode, string AssessmentName, decimal Score, decimal MaxScore);

record EvidenceRequest(string StudentId, string CourseCode, string CloCode, decimal Score, decimal MaxScore);

record AttainmentResult(string Code, decimal Percentage, bool Attained);

enum MasteryLevel
{
    Introduced,
    Reinforced,
    Mastered
}
