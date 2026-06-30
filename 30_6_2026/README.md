# AI Resume & Career Recommendation Demo

Phần mềm demo có Frontend và Backend cho các chủ đề:

- AI Resume Generation
- Resume Template Generation
- ATS Resume Optimization
- Personalized Resume Generation
- Academic-to-Career Mapping
- Career Recommendation System
- Swagger UI để test API

Demo này dùng dữ liệu in-memory và các rule engine đơn giản để mô phỏng AI. Hệ thống không gọi dịch vụ AI bên ngoài, nên có thể chạy offline sau khi restore package.

## Cấu trúc thư mục

```text
30_6_2026/
  AiResumeCareerDemo.csproj
  Program.cs
  AiResumeCareerDemo.http
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

Mở PowerShell tại thư mục project:

```powershell
cd C:\Users\Lenovo\DemoNet8Api\30_6_2026
dotnet restore
dotnet run --urls "http://localhost:5000"
```

Mở trình duyệt:

```text
Frontend: http://localhost:5000
Swagger:  http://localhost:5000/swagger
Health:   http://localhost:5000/api/health
```

Nếu cổng `5000` đang được dùng:

```powershell
dotnet run --urls "http://localhost:5010"
```

Sau đó mở:

```text
http://localhost:5010
http://localhost:5010/swagger
```

## Cách test trên Frontend

1. Mở `http://localhost:5000`.
2. Bấm `Nạp dữ liệu mẫu` nếu muốn dùng dữ liệu trong báo cáo.
3. Bấm `Chạy demo AI Resume`.
4. Xem kết quả gồm:
   - Điểm ATS.
   - Phân tích Job Description.
   - CV được sinh.
   - Kỹ năng suy ra từ dữ liệu học thuật.
   - Gợi ý nghề nghiệp và kỹ năng còn thiếu.

## Cách test API bằng Swagger

Mở:

```text
http://localhost:5000/swagger
```

Chọn `POST /api/pipeline/run`, bấm `Try it out`, nhập JSON:

```json
{
  "profile": {
    "fullName": "Nguyen Van A",
    "email": "nguyenvana@example.com",
    "major": "Công nghệ thông tin",
    "experienceLevel": "Student",
    "courses": [
      "Cơ sở dữ liệu",
      "Lập trình hướng đối tượng",
      "Lập trình web"
    ],
    "skills": [
      "C#",
      "SQL Server",
      "HTML",
      "CSS",
      "Git cơ bản"
    ],
    "projects": [
      "Website bán hàng có đăng nhập, giỏ hàng, quản lý sản phẩm"
    ],
    "activities": [
      "Làm việc nhóm trong đồ án môn học"
    ],
    "careerGoal": "Ứng tuyển Backend Developer Intern"
  },
  "targetJobTitle": "Backend Developer Intern",
  "jobDescription": "Yêu cầu C# hoặc Java, SQL database, REST API, Git, OOP. Có project web là lợi thế.",
  "language": "English",
  "preferOnePage": true
}
```

Response sẽ trả về toàn bộ pipeline:

- `jobAnalysis`: trích xuất kỹ năng từ mô tả công việc.
- `academicCareerMapping`: ánh xạ môn học, dự án, hoạt động sang kỹ năng nghề nghiệp.
- `template`: mẫu CV ATS-friendly.
- `resume`: bản nháp CV được sinh từ dữ liệu thật.
- `atsOptimization`: điểm ATS, kỹ năng khớp, kỹ năng thiếu và gợi ý tối ưu.
- `careerRecommendations`: gợi ý nghề nghiệp, kỹ năng mạnh, kỹ năng cần bổ sung.

## API chính

```text
GET  /api/health
GET  /api/demo/sample
GET  /api/demo/samples
GET  /api/demo/samples/{index}
GET  /api/skills/taxonomy
POST /api/job/analyze
POST /api/academic-career/map
POST /api/resume/templates/generate
POST /api/resume/generate
POST /api/resume/personalize
POST /api/ats/optimize
POST /api/careers/recommend
POST /api/pipeline/run
```

## Dữ liệu mẫu đa ngành

Frontend có combobox `Chọn hồ sơ mẫu` để nạp nhanh nhiều hồ sơ ứng viên:

- Công nghệ thông tin: Backend Developer Intern.
- Hệ thống thông tin quản lý: Data Analyst Intern.
- Marketing: Marketing Intern.
- Thiết kế đồ họa/UI-UX: UI/UX Designer Intern.
- Kế toán - Tài chính: Finance Analyst Intern.
- Quản trị nhân lực: HR Intern.

Có thể xem toàn bộ dữ liệu mẫu tại:

```text
GET /api/demo/samples
```

## Luồng kiến trúc demo

```text
User Profile + Academic Data + Job Description
        |
        v
Job Description Analyzer
        |
        v
Skill Mapping + Academic-to-Career Mapping
        |
        v
Resume Template Generation
        |
        v
AI Resume Generation / Personalized Resume Generation
        |
        v
ATS Resume Optimization
        |
        v
Career Recommendation System
        |
        v
Frontend hiển thị kết quả + Swagger test API
```

## Ghi chú về AI trong demo

Trong bản demo này, phần AI được mô phỏng bằng rule-based logic:

- Chuẩn hóa kỹ năng từ taxonomy đơn giản.
- Sinh summary và bullet point dựa trên dữ liệu người dùng cung cấp.
- Không tự bịa công ty, chứng chỉ, thành tích hoặc số liệu.
- Tính điểm ATS dựa trên skill coverage, keyword match, parseability và format safety.
- Gợi ý nghề nghiệp bằng content-based recommendation.

Khi triển khai thực tế, có thể thay `ResumeGenerator`, `JobDescriptionAnalyzer` hoặc `CareerRecommendationEngine` bằng LLM, embedding, semantic similarity, vector database và skill taxonomy đầy đủ hơn.
