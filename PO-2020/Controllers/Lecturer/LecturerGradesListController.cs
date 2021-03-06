﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PO_implementacja_StudiaPodyplomowe.Models;
using PO_implementacja_StudiaPodyplomowe.Models.Database;
using PO_implementacja_StudiaPodyplomowe.Models.Validators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PO_implementacja_StudiaPodyplomowe.Controllers.Lecturer
{
    public class LecturerGradesListController : Controller
    {
        private IDao manager = DaoSingleton.GetInstance().Dao;
        private static int staticParticipantId;
        private static string staticCourseId;

        public IActionResult Index()
        {
            List<Course> courses = manager.GetCourses(1, 1);
            ViewBag.coursesDataAvailable = courses.Count > 0;
            if (courses.Count > 0)
            {
                IEnumerable<SelectListItem> selectListCourses = from c in courses
                                                         select new SelectListItem
                                                         {
                                                             Value = c.CourseId.ToString(),
                                                             Text = c.Name.ToString()
                                                         };
                ViewData["Courses"] = new SelectList(selectListCourses, "Value", "Text", courses[0].CourseId.ToString());
            } else
            {
                ViewData["Courses"] = new SelectList(GetEmptySelectList(), "Value", "Text");
            }

            List<Models.Participant> participants = manager.GetParticipants();
            ViewBag.participantsDataAvailable = participants.Count > 0;
            if (participants.Count > 0)
            {
                IEnumerable<SelectListItem> selectListParticipants = from p in participants
                                                                     select new SelectListItem
                                                                     {
                                                                         Value = p.ParticipantId.ToString(),
                                                                         Text = p.Name.ToString() + " " + p.Surname.ToString()
                                                                     };
                ViewData["Participants"] = new SelectList(selectListParticipants, "Value", "Text");
            } else
            {
                ViewData["Participants"] = new SelectList(GetEmptySelectList(), "Value", "Text");
            }
           
            return View();
        }

        [HttpPost]
        public IActionResult Index(IFormCollection form)
        {
            var dict = new Dictionary<string, string>{
                { "course", form["CourseSelect"] },
                { "stud", form["StudSelect"] }
            };
            return RedirectToAction("Details", "LecturerGradesList", dict);
        }

        public IActionResult Edit(int id)
        {
            ViewBag.dataIsValid = true;

            PartialCourseGrade grade = manager.GetGrade(id);
            List<Models.Grade> grades = Enum.GetValues(typeof(Grade))
                .Cast<Grade>()
                .ToList();
            IEnumerable<SelectListItem> selectList = from g in grades
                                                     select new SelectListItem
                                                     {
                                                         Value = g.ToString(),
                                                         Text = GradeConverter.ParseGrade(g).ToString()
                                                     };
            ViewData["Grades"] = new SelectList(selectList, "Value", "Text", grade.GradeValue);

            return View(grade);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, IFormCollection form)
        {
            List<bool> fieldsValidation = GetGradeFieldsValidation(form);
            bool dataIsValid = ValidateGradeData(form, fieldsValidation);
            if (!dataIsValid)
            {
                var gradeValue = GradeConverter.GetGradeString(form["GradeId"]);
                ViewBag.fieldsValidation = fieldsValidation;

                PartialCourseGrade grade = manager.GetGrade(id);
                List<Grade> grades = Enum.GetValues(typeof(Grade))
                    .Cast<Grade>()
                    .ToList();
                IEnumerable<SelectListItem> selectList = from g in grades
                                                         select new SelectListItem
                                                         {
                                                             Value = g.ToString(),
                                                             Text = GradeConverter.ParseGrade(g).ToString()
                                                         };
                ViewData["Grades"] = new SelectList(selectList, "Value", "Text", gradeValue);
                ViewData["Date"] = form["DataTextField"];
                ViewData["Coment"] = form["ComentTextArea"];

                return View(grade);
            }

            PartialCourseGrade partialCourseGrade = new PartialCourseGrade();
            partialCourseGrade.PartialGradeId = id;
            partialCourseGrade.GradeDate = Convert.ToDateTime(form["DataTextField"]);
            partialCourseGrade.GradeValue = GradeConverter.GetGradeString(form["GradeId"]);
            partialCourseGrade.Comment = form["ComentTextArea"];
            manager.EditGrade(partialCourseGrade);
            return RedirectToAction("Index");
        }

        public IActionResult Add()
        {
            ViewBag.dataIsValid = true;

            List<Models.Grade> grades = Enum.GetValues(typeof(Grade))
                .Cast<Grade>()
                .ToList();
            IEnumerable<SelectListItem> selectList = from g in grades
                                                     select new SelectListItem
                                                     {
                                                         Value = g.ToString(),
                                                         Text = GradeConverter.ParseGrade(g).ToString()
                                                     };
            ViewData["Grades"] = new SelectList(selectList, "Value", "Text");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(IFormCollection form)
        {
            List<bool> fieldsValidation = GetGradeFieldsValidation(form);
            bool dataIsValid = ValidateGradeData(form, fieldsValidation);
            if (!dataIsValid)
            {
                var gradeValue = GradeConverter.GetGradeString(form["GradeId"]);

                ViewBag.fieldsValidation = fieldsValidation;
                ViewBag.form = form;
                List<Grade> grades = Enum.GetValues(typeof(Grade))
                .Cast<Grade>()
                .ToList();
                IEnumerable<SelectListItem> selectList = from g in grades
                                                         select new SelectListItem
                                                         {
                                                             Value = g.ToString(),
                                                             Text = GradeConverter.ParseGrade(g).ToString()
                                                         };
                ViewData["Grades"] = new SelectList(selectList, "Value", "Text", gradeValue);
                return View();
            }

            PartialCourseGrade partialCourseGrade = new PartialCourseGrade();
            int maxId = manager.GetMaxGradeId();
            partialCourseGrade.PartialGradeId = maxId + 1;
            partialCourseGrade.GradeDate = Convert.ToDateTime(form["DataTextField"]);
            partialCourseGrade.GradeValue = GradeConverter.GetGradeString(form["GradeId"]);
            partialCourseGrade.Comment = form["ComentTextArea"];

            Models.Participant participant = new Models.Participant();
            Course course = new Course();
            participant.ParticipantId = staticParticipantId;
            course.CourseId = staticCourseId;

            ParticipantGradeList list = manager.GetParticipantGradeList(participant, course);

            partialCourseGrade.ParticipantGradeList = list;



            manager.AddGrade(partialCourseGrade);
            return RedirectToAction("Index");
        }

        public IActionResult Details(Dictionary<string, string> dict)
        {
            Models.Participant participant = new Models.Participant();
            participant.ParticipantId = int.Parse(dict["stud"]);
            Course course = new Course();
            course.CourseId = dict["course"];
            staticParticipantId = participant.ParticipantId;
            staticCourseId = course.CourseId;

            List<PartialCourseGrade> grades = manager.GetParticipantsGrades(participant, course);
            ViewBag.gradesAreAvailable = grades.Count > 0;

            List<Tuple<DateTime, Grade, string, PartialCourseGrade>> finalList = new List<Tuple<DateTime, Grade, string, PartialCourseGrade>>();
            foreach (PartialCourseGrade partial in grades)
            {
                finalList.Add(new Tuple<DateTime, Grade, string, PartialCourseGrade>(partial.GradeDate, partial.GradeValue, partial.Comment, partial));
            }

            ViewData["finalList"] = finalList;

            //the same like in the index
            List<Course> courses = manager.GetCourses(1, 1);
            IEnumerable<SelectListItem> selectList = from c in courses
                                                     select new SelectListItem
                                                     {
                                                         Value = c.CourseId.ToString(),
                                                         Text = c.Name.ToString()
                                                     };
            ViewData["Courses"] = new SelectList(selectList, "Value", "Text", course.CourseId.ToString());

            List<Models.Participant> participants = manager.GetParticipants();
            IEnumerable<SelectListItem> selectListParticipants = from p in participants
                                                                 select new SelectListItem
                                                                 {
                                                                     Value = p.ParticipantId.ToString(),
                                                                     Text = p.Name.ToString() + " " + p.Surname.ToString()
                                                                 };
            ViewData["Participants"] = new SelectList(selectListParticipants, "Value", "Text", participant.ParticipantId.ToString());

            return View();
        }

        [HttpPost]
        public IActionResult Details(IFormCollection form)
        {
            var dict = new Dictionary<string, string>{
                { "course", form["CourseSelect"] },
                { "stud", form["StudSelect"] }
            };
            return RedirectToAction("Details", "LecturerGradesList", dict);
        }

        public IActionResult Delete(int id)
        {
            var dict = new Dictionary<string, string>{
                { "course", staticCourseId.ToString() },
                { "stud", staticParticipantId.ToString() }
            };

            manager.DeleteGrade(id);
            return RedirectToAction("Details", "LecturerGradesList", dict);
        }

        private List<bool> GetGradeFieldsValidation(IFormCollection form)
        {
            List<bool> fieldsValidation = new List<bool>();
            fieldsValidation.Add(DataValidator.DateIsValid(form["DataTextField"]));
            fieldsValidation.Add(DataValidator.FieldContentIsValid(form["GradeId"], maxLength: 7));
            fieldsValidation.Add(DataValidator.FieldContentIsValid(form["ComentTextArea"], maxLength: 255));

            return fieldsValidation;
        }

        private bool ValidateGradeData(IFormCollection form, List<bool> fieldsValidation)
        {
            bool dataIsValid = true;
            foreach (bool fieldValidation in fieldsValidation)
            {
                if (!fieldValidation)
                {
                    dataIsValid = false;
                }
            }
            ViewBag.dataIsValid = dataIsValid;
            ViewBag.form = form;
            return dataIsValid;
        }

        private List<SelectListItem> GetEmptySelectList()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();
            selectList.Add(new SelectListItem
            {
                Text = "Brak",
                Value = "0"
            });

            return selectList;
        }
    }
}
