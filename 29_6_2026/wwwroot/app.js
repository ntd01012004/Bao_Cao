const api = async (url, options) => {
  const response = await fetch(url, options);
  if (!response.ok) {
    throw new Error(await response.text());
  }
  return response.json();
};

const pct = value => `${Math.round(Number(value) * 100)}%`;

const profileLevelNames = {
  0: "Cần hỗ trợ",
  1: "Đang phát triển",
  2: "Thành thạo",
  3: "Nâng cao",
  AtRisk: "Cần hỗ trợ",
  Developing: "Đang phát triển",
  Proficient: "Thành thạo",
  Advanced: "Nâng cao"
};

const skillStatusNames = {
  0: "Cần hỗ trợ",
  1: "Đang hình thành",
  2: "Đạt yêu cầu",
  NeedsSupport: "Cần hỗ trợ",
  Emerging: "Đang hình thành",
  Mastered: "Đạt yêu cầu"
};

const assessmentTypeNames = {
  Quiz: "Trắc nghiệm",
  Lab: "Thực hành",
  DesignAssignment: "Bài thiết kế",
  RequirementBrief: "Tóm tắt yêu cầu",
  SprintReview: "Đánh giá sprint",
  Presentation: "Thuyết trình",
  ModelReport: "Báo cáo mô hình"
};

const relationshipNames = {
  Direct: "Trực tiếp",
  Supporting: "Bổ trợ"
};

const profileLabel = value => profileLevelNames[value] ?? String(value);
const skillStatusLabel = value => skillStatusNames[value] ?? String(value);
const assessmentTypeLabel = value => assessmentTypeNames[value] ?? String(value);
const relationshipLabel = value => relationshipNames[value] ?? String(value);

const profileClassName = value => ({
  0: "atrisk",
  1: "developing",
  2: "proficient",
  3: "advanced",
  AtRisk: "atrisk",
  Developing: "developing",
  Proficient: "proficient",
  Advanced: "advanced"
}[value] ?? "developing");

const state = {
  students: [],
  courses: [],
  skillMap: []
};

async function init() {
  const [students, courses, skills, skillMap, profiles, cohort, gaps] = await Promise.all([
    api("/api/students"),
    api("/api/courses"),
    api("/api/skills"),
    api("/api/skill-map"),
    api("/api/competency-profiles"),
    api("/api/analytics/cohort"),
    api("/api/analytics/skill-gaps")
  ]);

  state.students = students;
  state.courses = courses;
  state.skillMap = skillMap;

  renderMetrics(students, courses, skills, cohort);
  renderSkillMap(skillMap);
  renderFormOptions();
  renderDistribution(cohort.profileDistribution, students.length);
  renderGaps(gaps);
  renderProfiles(profiles);
}

function renderMetrics(students, courses, skills, cohort) {
  const metrics = [
    ["Sinh viên", students.length],
    ["Môn học", courses.length],
    ["Kỹ năng", skills.length],
    ["Năng lực TB", pct(cohort.averageSkillMastery)]
  ];

  document.getElementById("metrics").innerHTML = metrics.map(([label, value]) => `
    <div class="metric">
      <strong>${value}</strong>
      <span>${label}</span>
    </div>
  `).join("");
}

function renderSkillMap(items) {
  document.getElementById("skillMap").innerHTML = items.map(item => `
    <tr>
      <td><strong>${item.courseCode}</strong><br>${item.courseName}</td>
      <td>${assessmentTypeLabel(item.assessmentType)}</td>
      <td><strong>${item.skillCode}</strong><br>${item.skillName}</td>
      <td>${item.evidenceWeight}</td>
      <td>${relationshipLabel(item.relationship)}</td>
    </tr>
  `).join("");
}

function renderFormOptions() {
  document.getElementById("studentSelect").innerHTML = state.students
    .map(student => `<option value="${student.id}">${student.id} - ${student.fullName}</option>`)
    .join("");

  document.getElementById("courseSelect").innerHTML = state.courses
    .map(course => `<option value="${course.code}">${course.code} - ${course.name}</option>`)
    .join("");

  document.getElementById("courseSelect").addEventListener("change", updateAssessmentOptions);
  updateAssessmentOptions();
}

function updateAssessmentOptions() {
  const courseCode = document.getElementById("courseSelect").value;
  const assessmentTypes = [...new Set(state.skillMap
    .filter(item => item.courseCode === courseCode)
    .map(item => item.assessmentType))];

  document.getElementById("assessmentSelect").innerHTML = assessmentTypes
    .map(type => `<option value="${type}">${assessmentTypeLabel(type)}</option>`)
    .join("");
}

function renderDistribution(items, total) {
  document.getElementById("profileDistribution").innerHTML = items.map(item => {
    const width = total === 0 ? 0 : Math.round((item.count / total) * 100);
    return `
      <div class="bar-row">
        <strong>${profileLabel(item.profileLevel)}</strong>
        <div class="bar-track"><div class="bar-fill" style="width:${width}%"></div></div>
        <span>${item.count}</span>
      </div>
    `;
  }).join("");
}

function renderGaps(items) {
  document.getElementById("skillGaps").innerHTML = items.slice(0, 5).map(item => `
    <article class="gap-item">
      <header>
        <strong>${item.code} - ${item.name}</strong>
        <span>${pct(item.averageMastery)}</span>
      </header>
      <p class="muted">Sinh viên còn yếu: ${item.weakStudentCount} | Điểm khoảng trống: ${item.gapScore}</p>
    </article>
  `).join("");
}

function renderProfiles(profiles) {
  document.getElementById("profilesGrid").innerHTML = profiles.map(profile => {
    const profileClass = profileClassName(profile.profileLevel);
    return `
      <article class="profile-card">
        <header>
          <div>
            <strong>${profile.student.fullName}</strong>
            <p class="muted">${profile.student.id} | ${profile.student.cohort} | GPA ${profile.student.gpa} | Chuyên cần ${profile.student.attendanceRate}%</p>
          </div>
          <span class="tag ${profileClass}">${profileLabel(profile.profileLevel)}</span>
        </header>
        <p>Mức độ thành thạo tổng thể: <strong>${pct(profile.overallSkillMastery)}</strong></p>
        <div class="skill-list">
          ${profile.skills.map(skill => `
            <div class="skill-row">
              <span>${skill.skillCode} - ${skillStatusLabel(skill.status)}</span>
              <div class="progress" aria-label="${pct(skill.mastery)}">
                <span style="width:${pct(skill.mastery)}"></span>
              </div>
            </div>
          `).join("")}
        </div>
        <p class="muted">Gợi ý học tập: ${profile.recommendedResources.map(item => item.title).join(", ") || "Không có"}</p>
      </article>
    `;
  }).join("");
}

document.getElementById("inferenceForm").addEventListener("submit", async event => {
  event.preventDefault();
  const formData = new FormData(event.currentTarget);
  const payload = Object.fromEntries(formData.entries());
  payload.score = Number(payload.score);
  payload.maxScore = Number(payload.maxScore);

  const result = await api("/api/skill-inference/evaluate", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  });

  document.getElementById("inferenceResult").textContent = JSON.stringify(result, null, 2);
});

init().catch(error => {
  document.body.insertAdjacentHTML("afterbegin", `<div class="panel">${error.message}</div>`);
});
