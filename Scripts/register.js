form = document.getElementById("applicant-profile-form");
formParts = document.querySelectorAll(".form-part");
educationModalButton = document.querySelector(
    ".education-modal-button-add-row"
);
experienceModalButton = document.querySelector(
    ".experience-modal-button-add-row"
);
personalInformation = document.getElementById("personal-information");
education = document.getElementById("education");
experience = document.getElementById("experience");
resume = document.getElementById("resume");
personalInformationControl = document.getElementById(
    "personal-information-control"
);
educationControl = document.getElementById("education-control");
experienceControl = document.getElementById("experience-control");
resumeControl = document.getElementById("resume-control");
startyear = document.getElementById("inputstartyear4");
completionyear = document.getElementById("inputcompletionyear4");
degree = document.getElementById("inputdegree4");
specialization = document.getElementById("inputspecialization4");
university = document.getElementById("inputuniversity4");
educationTableBody = document.getElementById("education-table-body");
formNavControls = document.querySelectorAll(".form-nav-controls");
experienceTableBody = document.getElementById("experience-table-body");
jobtitle = document.getElementById("inputjobtitle4");
company = document.getElementById("inputcompany4");
startdate = document.getElementById("inputstartdate4");
enddate = document.getElementById("inputenddate4");
modalExp = document.getElementById("modalexp");
formSubmit = document.getElementById("form-submit");

function openFormPart(formPart) {
    formParts.forEach((element) => {
        element.classList.remove("showForm");
        element.classList.add("hideForm");
    });
    formPart.classList.remove("hideForm");
    formPart.classList.add("showForm");
}

function switchActive(control) {
    formNavControls.forEach((element) => {
        element.classList.remove("activeControl");
    });
    control.classList.add("activeControl");
}

personalInformationControl.addEventListener("click", function () {
    switchActive(personalInformationControl);
    openFormPart(personalInformation);
});

educationControl.addEventListener("click", function () {
    switchActive(educationControl);
    openFormPart(education);
});

experienceControl.addEventListener("click", function () {
    switchActive(experienceControl);
    openFormPart(experience);
});

resumeControl.addEventListener("click", function () {
    switchActive(resumeControl);
    openFormPart(resume);
});

function addRow(field1, field2, field3, field4, field5, tableBody) {
    tr = document.createElement("tr");

    if (field5) {
        tr.innerHTML = `
            <td><input type="date" class="startYearTB table-input" value="${field1.value}" required="required"/></td>
            <td><input type="date" class="completionYearTB table-input" value="${field2.value}" required="required"/></td>
            <td><input type="text" class="degreeTB table-input" value="${field3.value}" required="required"/></td>
            <td><input type="text" class="specializationTB table-input" value="${field4.value}" required="required"/></td>
            <td><input type="text" class="universityTB table-input" value="${field5.value}" required="required"/></td>        
            <td>
                <button type="button" class="trash-delete-row btn" style="margin-top: 10px;"><i class="fa fa-times" aria-hidden="true"></i></button>
            </td>`;
    } else if (!field5) {
        tr.innerHTML = `
            <td><input type="text" class="job-titleTB table-input" value="${field1.value}" required="required"/></td>
            <td><input type="text" class="companyTB table-input" value="${field2.value}" required="required"/></td>
            <td><input type="date" class="start-date-expTB table-input" value="${field3.value}" required="required"/></td>
            <td><input type="date" class="end-date-expTB table-input" value="${field4.value}" required="required"/></td>
            <td>
                <button type="button" class="trash-delete-row btn" style="margin-top: 10px;"><i class="fa fa-times" aria-hidden="true"></i></button>
            </td>`;
    }

    tableBody.appendChild(tr);
    field1.value = field2.value = field3.value = field4.value = field5.value = "";

    deleteRowBtns = document.querySelectorAll(".trash-delete-row");
    deleteRow(deleteRowBtns);
}

educationModalButton.addEventListener("click", function () {
    addRow(
        startyear,
        completionyear,
        degree,
        specialization,
        university,
        educationTableBody
    );
});

experienceModalButton.addEventListener("click", function () {
    addRow(jobtitle, company, startdate, enddate, "", experienceTableBody);
});

function deleteRow(deleteRowBtns) {
    deleteRowBtns.forEach((element) => {
        element.addEventListener("click", function () {
            element.parentNode.parentNode.remove();
        });
    });
}

function updateHiddenInput(tb_input, values, hiddenInput) {
    tb_input.forEach((el) => {
        values.push(el.value);
        hiddenInput.value = values.join(",");
    });
}

function save() {
    // Taking data from education table
    startYearTB = document.querySelectorAll(".startYearTB");
    completionYearTB = document.querySelectorAll(".completionYearTB");
    degreeTB = document.querySelectorAll(".degreeTB");
    specializationTB = document.querySelectorAll(".specializationTB");
    universityTB = document.querySelectorAll(".universityTB");
    startYearHiddenInput = document.querySelector(".start-year");
    completionYearHiddenInput = document.querySelector(".completion-year");
    degreeHiddenInput = document.querySelector(".degree");
    SpecializationHiddenInput = document.querySelector(".Specialization");
    UniversityHiddenInput = document.querySelector(".University");
    startYears = [];
    completionYears = [];
    degrees = [];
    specializations = [];
    universities = [];
    updateHiddenInput(startYearTB, startYears, startYearHiddenInput);
    updateHiddenInput(
        completionYearTB,
        completionYears,
        completionYearHiddenInput
    );
    updateHiddenInput(degreeTB, degrees, degreeHiddenInput);
    updateHiddenInput(
        specializationTB,
        specializations,
        SpecializationHiddenInput
    );
    updateHiddenInput(universityTB, universities, UniversityHiddenInput);
    // Taking data from experience table
    jobTitleTB = document.querySelectorAll(".job-titleTB");
    companyTB = document.querySelectorAll(".companyTB");
    startDateExpTB = document.querySelectorAll(".start-date-expTB");
    endDateExpTB = document.querySelectorAll(".end-date-expTB");
    jobTitleHiddenInput = document.querySelector(".job-title");
    companyHiddenInput = document.querySelector(".company");
    startDateExpHiddenInput = document.querySelector(".start-date-exp");
    endDateExpHiddenInput = document.querySelector(".end-date-exp");
    jobTitles = [];
    companies = [];
    startDateExps = [];
    endDateExps = [];
    updateHiddenInput(jobTitleTB, jobTitles, jobTitleHiddenInput);
    updateHiddenInput(companyTB, companies, companyHiddenInput);
    updateHiddenInput(startDateExpTB, startDateExps, startDateExpHiddenInput);
    updateHiddenInput(endDateExpTB, endDateExps, endDateExpHiddenInput);
}

function validate() {
    requiredInputs = document.querySelectorAll(".required");
    save();
    validatedInputs = [];
    notValidatedInputs = [];
    for (let i = 0; i < requiredInputs.length; i++) {
        if (requiredInputs[i].value) {
            validatedInputs.push(requiredInputs[i]);
        } else {
            notValidatedInputs.push(requiredInputs[i]);
        }
    }
    if (validatedInputs.length === requiredInputs.length) {
        form.submit();
    } else {
        alert("Please fill all the required fields.");
        validatedInputs.forEach((el) => {
            el.style.border = "1px solid #CED4DA";
        });
        notValidatedInputs.forEach((el) => {
            el.style.border = "solid 1px red";
        });
    }
}

formSubmit.addEventListener("click", validate);
