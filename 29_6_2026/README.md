# Student Skill Intelligence Demo

Demo ASP.NET Core co Frontend va Backend cho cac chu de:

- Student Academic Data Modeling
- Skill Mapping
- Competency Profile Generation
- Skill Inference
- Swagger UI de test API

Project dung du lieu in-memory, khong can database.

## Cau truc

```text
29_6_2026/
  StudentSkillIntelligenceDemo.csproj
  Program.cs
  StudentSkillIntelligenceDemo.http
  README.md
  wwwroot/
    index.html
    styles.css
    app.js
```

## Yeu cau

- .NET SDK 10.0 hoac moi hon
- Trinh duyet web

Kiem tra SDK:

```powershell
dotnet --version
```

## Cach chay

```powershell
cd C:\Users\Lenovo\DemoNet8Api\29_6_2026
dotnet restore
dotnet run --urls "http://localhost:5000"
```

Mo trinh duyet:

```text
Frontend: http://localhost:5000
Swagger:  http://localhost:5000/swagger
Health:   http://localhost:5000/api/health
```

Neu port `5000` dang duoc dung:

```powershell
dotnet run --urls "http://localhost:5010"
```

Sau do mo:

```text
http://localhost:5010
http://localhost:5010/swagger
```

## API chinh

```text
GET  /api/health
GET  /api/students
GET  /api/students/{studentId}/academic-data
GET  /api/courses
GET  /api/skills
GET  /api/skill-map
GET  /api/competency-profiles
GET  /api/competency-profiles/{studentId}
GET  /api/skill-inference/{studentId}
POST /api/skill-inference/evaluate
GET  /api/analytics/cohort
GET  /api/analytics/skill-gaps
```

## Vi du test POST tren Swagger

Mo `http://localhost:5000/swagger`, chon `POST /api/skill-inference/evaluate`, bam `Try it out`, nhap:

```json
{
  "studentId": "S003",
  "courseCode": "SE410",
  "assessmentType": "ModelReport",
  "score": 8.2,
  "maxScore": 10
}
```

Response tra ve cac skill duoc suy luan tu assessment moi, muc do dong gop evidence va predicted level.

## Mo hinh xu ly

```text
Student Academic Data
  GPA, attendance, enrollments, assessments, learning activities
        |
        v
Skill Mapping
  Course + assessment type -> skill + evidence weight
        |
        v
Skill Inference
  Assessment score + activity signal + prerequisite signal
        |
        v
Competency Profile Generation
  Overall mastery, skill status, confidence, recommendations
```

Nguong `Mastered` mac dinh la `70%`, `Emerging` la `55%`.
