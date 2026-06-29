# Student Skill Intelligence Demo

Demo ASP.NET Core có Frontend và Backend:

- Student Academic Data Modeling
- Skill Mapping
- Competency Profile Generation
- Skill Inference
- Swagger UI test API

Project dùng dữ liệu in-memory, không cần database.

## Cấu trúc

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

## Yêu cầu

- .NET SDK 10.0 hoặc mới hơn
- Trình duyệt web

Kiểm tra SDK:

```powershell
dotnet --version
```

## Cách chạy

```powershell
cd C:\Users\Lenovo\DemoNet8Api\29_6_2026
dotnet restore
dotnet run --urls "http://localhost:5000"
```

Mở trình duyệt:

```text
Frontend: http://localhost:5000
Swagger:  http://localhost:5000/swagger
Health:   http://localhost:5000/api/health
```

Nếu port `5000` đang được dùng:

```powershell
dotnet run --urls "http://localhost:5010"
```

Sau đó mở:

```text
http://localhost:5010
http://localhost:5010/swagger
```

## API chính

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

## Ví dụ test POST trên Swagger

Mở `http://localhost:5000/swagger`, chọn `POST /api/skill-inference/evaluate`, bấm `Try it out`, nhập:

```json
{
  "studentId": "S003",
  "courseCode": "SE410",
  "assessmentType": "ModelReport",
  "score": 8.2,
  "maxScore": 10
}
```

Response trả về các skill được suy luận từ assessment mới, mức độ đóng góp evidence và predicted level.

## Mô hình xử lý

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

Ngưỡng `Mastered` mặc định la `70%`, `Emerging` la `55%`.
