# OBE Competency Framework Mini API

Mini project ASP.NET Core minh hoa Outcome-Based Education (OBE) voi:

- PLO (Program Learning Outcomes): chuan dau ra chuong trinh.
- CLO (Course Learning Outcomes): chuan dau ra hoc phan.
- Competency Framework: khung nang luc gan voi CLO/PLO.
- Curriculum Map: ma tran lien ket CLO -> PLO -> competency.
- Attainment: tinh ty le dat CLO, PLO va competency tu minh chung danh gia.

> May hien tai dang dung .NET SDK 10, nen project target `net10.0`.

## Cau truc

```text
25_6_2026/
  ObeCompetencyDemo.csproj
  Program.cs
  ObeCompetencyDemo.http
  README.md
```

## Chay project

```powershell
cd C:\Users\Lenovo\DemoNet8Api\25_6_2026
dotnet run
```

Mac dinh app se chay tren URL duoc in ra trong terminal, thuong la:

```text
http://localhost:5000
```

## Endpoint chinh

```text
GET  /api/program
GET  /api/courses
GET  /api/plos
GET  /api/clos
GET  /api/clos?courseCode=SE101
GET  /api/competencies
GET  /api/curriculum-map
GET  /api/attainment/summary
GET  /api/attainment/student/S001
POST /api/attainment/evaluate
```

## Vi du POST evaluate

```json
{
  "studentId": "S003",
  "courseCode": "SE305",
  "cloCode": "CLO5",
  "score": 8.2,
  "maxScore": 10
}
```

Response cho biet sinh vien co dat CLO hay khong, dong thoi tra ve cac PLO va competency lien quan.

## Mo hinh OBE trong project

```text
Assessment Evidence
        |
        v
       CLO
        |
        v
       PLO
        |
        v
   Competency
```

Attainment threshold mac dinh la `70%`. PLO va competency attainment duoc tinh bang trung binh co trong so tu cac CLO mapping.
