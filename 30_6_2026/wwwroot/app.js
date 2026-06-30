const $ = (id) => document.getElementById(id);

let sampleRequests = [];

const text = {
  loadingSamples: "\u0110ang n\u1ea1p danh s\u00e1ch h\u1ed3 s\u01a1 m\u1eabu...",
  sampleLoaded: "\u0110\u00e3 n\u1ea1p h\u1ed3 s\u01a1 m\u1eabu.",
  running: "\u0110ang g\u1ecdi API /api/pipeline/run...",
  done: "\u0110\u00e3 ch\u1ea1y xong pipeline demo.",
  ready: "S\u1eb5n s\u00e0ng ch\u1ea1y pipeline.",
  error: "L\u1ed7i",
  initError: "L\u1ed7i kh\u1edfi t\u1ea1o",
  strongSkills: "K\u1ef9 n\u0103ng m\u1ea1nh",
  missingSkills: "C\u1ea7n b\u1ed5 sung",
  noData: "Ch\u01b0a c\u00f3 d\u1eef li\u1ec7u",
  noLargeGap: "Kh\u00f4ng c\u00f3 kho\u1ea3ng tr\u1ed1ng k\u1ef9 n\u0103ng l\u1edbn"
};

const listFromText = (value) => value
  .split(/\r?\n/)
  .map((item) => item.trim())
  .filter(Boolean);

const setStatus = (message) => {
  $("status").textContent = message;
};

const chip = (value) => {
  const item = document.createElement("span");
  item.className = "chip";
  item.textContent = value;
  return item;
};

const renderChips = (id, values) => {
  const root = $(id);
  root.innerHTML = "";
  values.forEach((value) => root.appendChild(chip(value)));
};

const renderList = (id, values) => {
  const root = $(id);
  root.innerHTML = "";
  values.forEach((value) => {
    const li = document.createElement("li");
    li.textContent = value;
    root.appendChild(li);
  });
};

const buildRequest = () => ({
  profile: {
    fullName: $("fullName").value,
    email: $("email").value,
    major: $("major").value,
    experienceLevel: $("experienceLevel").value,
    courses: listFromText($("courses").value),
    skills: listFromText($("skills").value),
    projects: listFromText($("projects").value),
    activities: listFromText($("activities").value),
    careerGoal: $("careerGoal").value
  },
  targetJobTitle: $("targetJobTitle").value,
  jobDescription: $("jobDescription").value,
  language: "English",
  preferOnePage: true
});

const fillForm = (request) => {
  $("fullName").value = request.profile.fullName;
  $("email").value = request.profile.email;
  $("major").value = request.profile.major;
  $("experienceLevel").value = request.profile.experienceLevel;
  $("courses").value = request.profile.courses.join("\n");
  $("skills").value = request.profile.skills.join("\n");
  $("projects").value = request.profile.projects.join("\n");
  $("activities").value = request.profile.activities.join("\n");
  $("careerGoal").value = request.profile.careerGoal;
  $("targetJobTitle").value = request.targetJobTitle;
  $("jobDescription").value = request.jobDescription;
};

const renderResult = (data) => {
  const ats = data.atsOptimization;
  const resume = data.resume;
  const job = data.jobAnalysis;

  $("overallScore").textContent = ats.overallScore;
  $("skillCoverage").textContent = `${ats.skillCoverageScore}%`;
  $("keywordMatch").textContent = `${ats.keywordMatchScore}%`;
  $("parseability").textContent = `${ats.parseabilityScore}%`;
  $("formatSafety").textContent = `${ats.formatSafetyScore}%`;

  $("jobSummary").textContent = `${job.jobTitle} - ${job.seniority}`;
  renderChips("requiredSkills", job.requiredSkills);

  $("resumeTitle").textContent = `${resume.candidateName} - ${resume.targetJobTitle}`;
  $("resumeSummary").textContent = resume.summary;
  renderList("sections", resume.sections);
  renderList("projectBullets", resume.projectBullets);
  renderChips("resumeSkills", resume.skills);

  const evidence = data.academicCareerMapping.evidence.map((item) =>
    `${item.source} -> ${item.skill}: ${item.resumeSuggestion}`);
  renderList("evidenceList", evidence);

  const careerRoot = $("careerList");
  careerRoot.innerHTML = "";
  data.careerRecommendations.recommendations.forEach((item) => {
    const card = document.createElement("div");
    card.className = "career-item";

    const title = document.createElement("strong");
    title.innerHTML = `${item.roleName} <span>${item.matchScore}%</span>`;
    card.appendChild(title);

    const skills = document.createElement("p");
    skills.textContent = `${text.strongSkills}: ${item.strongSkills.join(", ") || text.noData}`;
    card.appendChild(skills);

    const gaps = document.createElement("p");
    gaps.textContent = `${text.missingSkills}: ${item.missingSkills.join(", ") || text.noLargeGap}`;
    card.appendChild(gaps);

    careerRoot.appendChild(card);
  });

  renderList("advice", ats.advice.concat(resume.humanReviewNotes));
};

const runPipeline = async () => {
  setStatus(text.running);
  const response = await fetch("/api/pipeline/run", {
    method: "POST",
    headers: { "Content-Type": "application/json; charset=utf-8" },
    body: JSON.stringify(buildRequest())
  });

  if (!response.ok) {
    const responseText = await response.text();
    throw new Error(responseText || `HTTP ${response.status}`);
  }

  const data = await response.json();
  renderResult(data);
  setStatus(text.done);
};

const loadSelectedSample = () => {
  const index = Number($("sampleProfile").value || 0);
  const request = sampleRequests[index] || sampleRequests[0];
  if (!request) {
    return;
  }

  fillForm(request);
  setStatus(text.sampleLoaded);
};

const loadSamples = async () => {
  setStatus(text.loadingSamples);
  const response = await fetch("/api/demo/samples");
  sampleRequests = await response.json();

  const select = $("sampleProfile");
  select.innerHTML = "";
  sampleRequests.forEach((request, index) => {
    const option = document.createElement("option");
    option.value = index;
    option.textContent = `${request.profile.fullName} - ${request.profile.major} - ${request.targetJobTitle}`;
    select.appendChild(option);
  });

  loadSelectedSample();
};

$("runButton").addEventListener("click", () => {
  runPipeline().catch((error) => setStatus(`${text.error}: ${error.message}`));
});

$("loadSampleButton").addEventListener("click", () => {
  loadSelectedSample();
  runPipeline().catch((error) => setStatus(`${text.error}: ${error.message}`));
});

$("sampleProfile").addEventListener("change", () => {
  loadSelectedSample();
  runPipeline().catch((error) => setStatus(`${text.error}: ${error.message}`));
});

loadSamples()
  .then(runPipeline)
  .catch((error) => setStatus(`${text.initError}: ${error.message}`));
