# EDM Learning Analytics Mini API

Mini project ASP.NET Core minh hoa:

- Competency Assessment: danh gia muc do thanh thao theo tung nang luc.
- Learning Analytics: tong hop diem, thoi gian hoc, ngay hoat dong va muc do rui ro.
- Student Modeling: tao ho so hoc tap cho tung sinh vien.
- Educational Data Mining (EDM): ap dung cac rule don gian de phat hien rui ro va goi y can thiep.

> May hien tai dang dung .NET SDK 10, nen project target `net10.0`.

## Cau truc

```text
26_6_2026/
  EdmLearningAnalyticsDemo.csproj
  Program.cs
  EdmLearningAnalyticsDemo.http
  README.md
```

## Chay project

```powershell
cd C:\Users\Lenovo\DemoNet8Api\26_6_2026
dotnet run
```

Mac dinh app se in URL trong terminal, thuong la:

```text
http://localhost:5000
```

## Endpoint chinh

```text
GET  /
GET  /api/competencies
GET  /api/resources
GET  /api/students
GET  /api/student-models
GET  /api/student-models/{studentId}
GET  /api/analytics/cohort
GET  /api/analytics/competency-gaps
GET  /api/edm/risk-rules
GET  /api/edm/interventions
POST /api/assessment/evaluate
```

## Vi du POST evaluate

```json
{
  "studentId": "S002",
  "competencyCode": "C02",
  "score": 5.8,
  "maxScore": 10
}
```

Response cho biet sinh vien co dat competency hay khong, co sinh risk signal hay khong, va nen hoc lai resource nao.

## Luong xu ly

```text
Assessment Events + Learning Events
        |
        v
Competency Mastery + Engagement Profile
        |
        v
Student Model
        |
        v
EDM Risk Rules + Recommended Resources
```

Nguong dat nang luc mac dinh la `70%`. Sinh vien co mastery duoi `60%`, it thoi gian hoc, missed activity, hoac competency gap se duoc gan risk signal.
