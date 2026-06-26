var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

const decimal MasteryThreshold = 0.70m;
const decimal RiskThreshold = 0.60m;

var competencies = new List<Competency>
{
    new("C01", "Conceptual Understanding", "Understand key ideas, terminology and principles."),
    new("C02", "Problem Solving", "Apply concepts to solve authentic learning tasks."),
    new("C03", "Data Interpretation", "Read evidence, explain patterns and justify decisions."),
    new("C04", "Self-Regulated Learning", "Plan study time, submit work consistently and recover from feedback.")
};

var learningResources = new List<LearningResource>
{
    new("R01", "Video: Regression Basics", "C01"),
    new("R02", "Lab: Prediction With Spreadsheets", "C02"),
    new("R03", "Dashboard Case Study", "C03"),
    new("R04", "Reflection Checklist", "C04")
};

var students = new List<Student>
{
    new("S001", "An Nguyen", "SE-K18"),
    new("S002", "Binh Tran", "SE-K18"),
    new("S003", "Chi Le", "SE-K19"),
    new("S004", "Dung Pham", "SE-K19")
};

var assessmentEvents = new List<AssessmentEvent>
{
    new("S001", "C01", "Quiz 1", 8.5m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-18))),
    new("S001", "C02", "Prediction Lab", 7.5m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-12))),
    new("S001", "C03", "Case Analysis", 8.0m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-6))),
    new("S001", "C04", "Learning Journal", 7.0m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-3))),
    new("S002", "C01", "Quiz 1", 6.0m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-18))),
    new("S002", "C02", "Prediction Lab", 5.5m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-12))),
    new("S002", "C03", "Case Analysis", 6.5m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-6))),
    new("S002", "C04", "Learning Journal", 5.0m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-3))),
    new("S003", "C01", "Quiz 1", 7.5m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-18))),
    new("S003", "C02", "Prediction Lab", 6.8m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-12))),
    new("S003", "C03", "Case Analysis", 7.2m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-6))),
    new("S003", "C04", "Learning Journal", 8.0m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-3))),
    new("S004", "C01", "Quiz 1", 9.0m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-18))),
    new("S004", "C02", "Prediction Lab", 8.5m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-12))),
    new("S004", "C03", "Case Analysis", 8.8m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-6))),
    new("S004", "C04", "Learning Journal", 9.2m, 10m, DateOnly.FromDateTime(DateTime.Today.AddDays(-3)))
};

var learningEvents = new List<LearningEvent>
{
    new("S001", "R01", LearningAction.Watched, 42, DateOnly.FromDateTime(DateTime.Today.AddDays(-20))),
    new("S001", "R02", LearningAction.Practiced, 65, DateOnly.FromDateTime(DateTime.Today.AddDays(-13))),
    new("S001", "R03", LearningAction.Submitted, 50, DateOnly.FromDateTime(DateTime.Today.AddDays(-5))),
    new("S002", "R01", LearningAction.Watched, 20, DateOnly.FromDateTime(DateTime.Today.AddDays(-20))),
    new("S002", "R02", LearningAction.Practiced, 25, DateOnly.FromDateTime(DateTime.Today.AddDays(-13))),
    new("S002", "R04", LearningAction.Missed, 0, DateOnly.FromDateTime(DateTime.Today.AddDays(-2))),
    new("S003", "R01", LearningAction.Watched, 36, DateOnly.FromDateTime(DateTime.Today.AddDays(-20))),
    new("S003", "R02", LearningAction.Practiced, 40, DateOnly.FromDateTime(DateTime.Today.AddDays(-13))),
    new("S003", "R04", LearningAction.Submitted, 45, DateOnly.FromDateTime(DateTime.Today.AddDays(-2))),
    new("S004", "R01", LearningAction.Watched, 55, DateOnly.FromDateTime(DateTime.Today.AddDays(-20))),
    new("S004", "R02", LearningAction.Practiced, 80, DateOnly.FromDateTime(DateTime.Today.AddDays(-13))),
    new("S004", "R03", LearningAction.Submitted, 70, DateOnly.FromDateTime(DateTime.Today.AddDays(-5))),
    new("S004", "R04", LearningAction.Submitted, 35, DateOnly.FromDateTime(DateTime.Today.AddDays(-2)))
};

app.MapGet("/", () => Results.Ok(new
{
    name = "EDM Learning Analytics Mini API",
    topics = new[]
    {
        "Competency Assessment",
        "Learning Analytics",
        "Student Modeling",
        "Educational Data Mining"
    },
    endpoints = new[]
    {
        "/api/competencies",
        "/api/resources",
        "/api/students",
        "/api/student-models",
        "/api/student-models/{studentId}",
        "/api/analytics/cohort",
        "/api/analytics/competency-gaps",
        "/api/edm/risk-rules",
        "/api/edm/interventions",
        "/api/assessment/evaluate"
    }
}));

app.MapGet("/api/competencies", () => Results.Ok(competencies));

app.MapGet("/api/resources", () => Results.Ok(learningResources));

app.MapGet("/api/students", () => Results.Ok(students));

app.MapGet("/api/student-models", () =>
{
    var models = students.Select(student => BuildStudentModel(student, assessmentEvents, learningEvents, learningResources, competencies));
    return Results.Ok(models);
});

app.MapGet("/api/student-models/{studentId}", (string studentId) =>
{
    var student = students.FirstOrDefault(item => item.Id.Equals(studentId, StringComparison.OrdinalIgnoreCase));
    if (student is null)
    {
        return Results.NotFound(new { message = $"Student '{studentId}' was not found." });
    }

    return Results.Ok(BuildStudentModel(student, assessmentEvents, learningEvents, learningResources, competencies));
});

app.MapGet("/api/analytics/cohort", () =>
{
    var models = students
        .Select(student => BuildStudentModel(student, assessmentEvents, learningEvents, learningResources, competencies))
        .ToList();

    return Results.Ok(new
    {
        studentCount = models.Count,
        averageMastery = decimal.Round(models.Average(model => model.OverallMastery), 4),
        averageEngagementMinutes = Math.Round(models.Average(model => model.Engagement.TotalMinutes), 2),
        riskDistribution = models.GroupBy(model => model.RiskLevel)
            .Select(group => new { riskLevel = group.Key, count = group.Count() })
            .OrderBy(item => item.riskLevel),
        competencyAverages = competencies.Select(competency =>
        {
            var average = models
                .SelectMany(model => model.CompetencyMastery)
                .Where(result => result.CompetencyCode == competency.Code)
                .Average(result => result.Mastery);

            return new
            {
                competency.Code,
                competency.Name,
                averageMastery = decimal.Round(average, 4),
                attained = average >= MasteryThreshold
            };
        })
    });
});

app.MapGet("/api/analytics/competency-gaps", () =>
{
    var models = students
        .Select(student => BuildStudentModel(student, assessmentEvents, learningEvents, learningResources, competencies))
        .ToList();

    var gaps = competencies.Select(competency =>
    {
        var relatedMastery = models
            .SelectMany(model => model.CompetencyMastery)
            .Where(result => result.CompetencyCode == competency.Code)
            .ToList();

        var average = relatedMastery.Average(result => result.Mastery);
        var atRiskCount = relatedMastery.Count(result => result.Mastery < RiskThreshold);

        return new
        {
            competency.Code,
            competency.Name,
            averageMastery = decimal.Round(average, 4),
            atRiskCount,
            gapScore = decimal.Round((1 - average) + (atRiskCount * 0.10m), 4)
        };
    })
    .OrderByDescending(item => item.gapScore);

    return Results.Ok(gaps);
});

app.MapGet("/api/edm/risk-rules", () => Results.Ok(new[]
{
    new RiskRule("LowMastery", "Overall mastery below 60%."),
    new RiskRule("LowEngagement", "Total learning time below 80 minutes."),
    new RiskRule("MissedActivity", "At least one required learning activity was missed."),
    new RiskRule("CompetencyGap", "Any competency mastery below 60%.")
}));

app.MapGet("/api/edm/interventions", () =>
{
    var recommendations = students
        .Select(student => BuildStudentModel(student, assessmentEvents, learningEvents, learningResources, competencies))
        .Where(model => model.RiskLevel != RiskLevel.Low)
        .Select(model => new
        {
            model.Student.Id,
            model.Student.FullName,
            model.RiskLevel,
            weakestCompetency = model.CompetencyMastery.OrderBy(result => result.Mastery).First(),
            recommendedResources = RecommendResourcesForModel(model, learningResources),
            model.Signals
        });

    return Results.Ok(recommendations);
});

app.MapPost("/api/assessment/evaluate", (AssessmentRequest request) =>
{
    if (request.Score < 0 || request.MaxScore <= 0 || request.Score > request.MaxScore)
    {
        return Results.BadRequest(new { message = "Score must be between 0 and MaxScore, and MaxScore must be greater than 0." });
    }

    var student = students.FirstOrDefault(item => item.Id.Equals(request.StudentId, StringComparison.OrdinalIgnoreCase));
    if (student is null)
    {
        return Results.BadRequest(new { message = $"Unknown student '{request.StudentId}'." });
    }

    var competency = competencies.FirstOrDefault(item => item.Code.Equals(request.CompetencyCode, StringComparison.OrdinalIgnoreCase));
    if (competency is null)
    {
        return Results.BadRequest(new { message = $"Unknown competency '{request.CompetencyCode}'." });
    }

    var mastery = decimal.Round(request.Score / request.MaxScore, 4);

    return Results.Ok(new
    {
        request.StudentId,
        student.FullName,
        request.CompetencyCode,
        competency.Name,
        mastery,
        attained = mastery >= MasteryThreshold,
        riskSignal = mastery < RiskThreshold ? "CompetencyGap" : "NoImmediateRisk",
        recommendation = mastery < MasteryThreshold
            ? RecommendResourcesForCompetency(request.CompetencyCode, learningResources)
            : Array.Empty<LearningResource>()
    });
});

app.Run();

static StudentModel BuildStudentModel(
    Student student,
    IEnumerable<AssessmentEvent> assessments,
    IEnumerable<LearningEvent> learningEvents,
    IReadOnlyCollection<LearningResource> resources,
    IReadOnlyCollection<Competency> competencies)
{
    var studentAssessments = assessments
        .Where(assessment => assessment.StudentId.Equals(student.Id, StringComparison.OrdinalIgnoreCase))
        .ToList();

    var studentLearningEvents = learningEvents
        .Where(learningEvent => learningEvent.StudentId.Equals(student.Id, StringComparison.OrdinalIgnoreCase))
        .ToList();

    var mastery = competencies.Select(competency =>
    {
        var relatedAssessments = studentAssessments
            .Where(assessment => assessment.CompetencyCode == competency.Code)
            .ToList();

        var score = relatedAssessments.Count == 0
            ? 0
            : relatedAssessments.Sum(assessment => assessment.Score) / relatedAssessments.Sum(assessment => assessment.MaxScore);

        return new CompetencyMastery(
            competency.Code,
            competency.Name,
            decimal.Round(score, 4),
            score >= 0.70m);
    }).ToList();

    var engagement = BuildEngagementProfile(studentLearningEvents);
    var signals = BuildRiskSignals(mastery, engagement);
    var overallMastery = mastery.Count == 0 ? 0 : mastery.Average(result => result.Mastery);
    var riskLevel = ClassifyRisk(overallMastery, signals.Count);

    return new StudentModel(
        student,
        decimal.Round(overallMastery, 4),
        riskLevel,
        mastery,
        engagement,
        signals,
        RecommendResourcesForMastery(mastery, resources));
}

static EngagementProfile BuildEngagementProfile(IReadOnlyCollection<LearningEvent> learningEvents)
{
    var totalMinutes = learningEvents.Sum(item => item.DurationMinutes);
    var missedActivities = learningEvents.Count(item => item.Action == LearningAction.Missed);
    var activeDays = learningEvents.Select(item => item.OccurredOn).Distinct().Count();
    var lastActivity = learningEvents.Count == 0
        ? (DateOnly?)null
        : learningEvents.Max(item => item.OccurredOn);

    return new EngagementProfile(totalMinutes, activeDays, missedActivities, lastActivity);
}

static List<string> BuildRiskSignals(IReadOnlyCollection<CompetencyMastery> mastery, EngagementProfile engagement)
{
    var signals = new List<string>();

    if (mastery.Average(item => item.Mastery) < 0.60m)
    {
        signals.Add("LowMastery");
    }

    if (engagement.TotalMinutes < 80)
    {
        signals.Add("LowEngagement");
    }

    if (engagement.MissedActivities > 0)
    {
        signals.Add("MissedActivity");
    }

    if (mastery.Any(item => item.Mastery < 0.60m))
    {
        signals.Add("CompetencyGap");
    }

    return signals;
}

static RiskLevel ClassifyRisk(decimal overallMastery, int signalCount)
{
    if (overallMastery < 0.60m || signalCount >= 3)
    {
        return RiskLevel.High;
    }

    if (overallMastery < 0.70m || signalCount > 0)
    {
        return RiskLevel.Medium;
    }

    return RiskLevel.Low;
}

static IReadOnlyCollection<LearningResource> RecommendResourcesForModel(
    StudentModel model,
    IReadOnlyCollection<LearningResource> resources)
{
    return RecommendResourcesForMastery(model.CompetencyMastery, resources);
}

static IReadOnlyCollection<LearningResource> RecommendResourcesForMastery(
    IReadOnlyCollection<CompetencyMastery> mastery,
    IReadOnlyCollection<LearningResource> resources)
{
    var weakCompetencies = mastery
        .Where(result => result.Mastery < 0.70m)
        .Select(result => result.CompetencyCode)
        .ToHashSet();

    return resources
        .Where(resource => weakCompetencies.Contains(resource.CompetencyCode))
        .ToList();
}

static IReadOnlyCollection<LearningResource> RecommendResourcesForCompetency(
    string competencyCode,
    IReadOnlyCollection<LearningResource> resources)
{
    return resources
        .Where(resource => resource.CompetencyCode.Equals(competencyCode, StringComparison.OrdinalIgnoreCase))
        .ToList();
}

record Competency(string Code, string Name, string Description);

record LearningResource(string Id, string Title, string CompetencyCode);

record Student(string Id, string FullName, string Cohort);

record AssessmentEvent(string StudentId, string CompetencyCode, string AssessmentName, decimal Score, decimal MaxScore, DateOnly AssessedOn);

record LearningEvent(string StudentId, string ResourceId, LearningAction Action, int DurationMinutes, DateOnly OccurredOn);

record CompetencyMastery(string CompetencyCode, string CompetencyName, decimal Mastery, bool Attained);

record EngagementProfile(int TotalMinutes, int ActiveDays, int MissedActivities, DateOnly? LastActivityOn);

record StudentModel(
    Student Student,
    decimal OverallMastery,
    RiskLevel RiskLevel,
    IReadOnlyCollection<CompetencyMastery> CompetencyMastery,
    EngagementProfile Engagement,
    IReadOnlyCollection<string> Signals,
    IReadOnlyCollection<LearningResource> RecommendedResources);

record AssessmentRequest(string StudentId, string CompetencyCode, decimal Score, decimal MaxScore);

record RiskRule(string Code, string Description);

enum LearningAction
{
    Watched,
    Practiced,
    Submitted,
    Missed
}

enum RiskLevel
{
    Low,
    Medium,
    High
}
