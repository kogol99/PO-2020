using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PO_implementacja_StudiaPodyplomowe.Models;
using PO_implementacja_StudiaPodyplomowe.Models.Database;
using PO_implementacja_StudiaPodyplomowe.Models.Validators;
using System;
using System.Collections.Generic;

namespace PO_implementacja_StudiaPodyplomowe.Controllers.Lecturer
{
    public class LecturerFinalThesisListController : Controller
    {
        private IDao manager = DaoSingleton.GetInstance().Dao;

        public IActionResult Index()
        {
            List<FinalThesisReview> reviews = manager.GetReviews(1);

            ViewBag.isDataAvailable = reviews.Count > 0;
            List<string> topics = new List<string>();
            for (int i = 0; i < reviews.Count; i++)
            {
                SubmissionThesis submission = manager.GetSubmissionForThesisId(reviews[i].FinalThesis.FinalThesisId);
                topics.Add(submission.ThesisTopic);
            }
            ViewBag.topics = topics;

            return View(reviews);
        }

        public IActionResult Confirm(int id)
        {
            manager.EditReviewStatus(id, ThesisStatus.APPROVED);
            return RedirectToAction("Index");
        }

        public IActionResult Discard(int id)
        {
            manager.EditReviewStatus(id, ThesisStatus.DISCARD);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            UpdateTopicAndName(id);
            ViewBag.dataIsValid = true;

            return View(manager.GetReview(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, IFormCollection form)
        {
            List<bool> fieldsValidation = GetReviewFieldsValidation(form);
            bool dataIsValid = true;
            foreach (bool fieldValidation in fieldsValidation)
            {
                if (!fieldValidation)
                {
                    dataIsValid = false;
                }
            }

            UpdateTopicAndName(id);
            ViewBag.dataIsValid = dataIsValid;
            ViewBag.form = form;

            if (!dataIsValid)
            {
                ViewBag.fieldsValidation = fieldsValidation;
                return View();
            }

            FinalThesisReview review = new FinalThesisReview();
            review.FormId = id;
            review.TitleCompability = form["TitleCompability"];
            review.ThesisStructureComment = form["ThesisStructureComment"];
            review.NewProblem = form["NewProblem"];
            review.SourcesUse = form["SourcesUse"];
            review.FormalWorkSide = form["FormalWorkSide"];
            review.WayToUse = form["WayToUse"];
            review.SubstantiveThesisGrade = form["SubstantiveThesisGrade"];
            review.ThesisGrade = form["ThesisGrade"];
            review.FormDate = DateTime.Parse(form["FormDate"]);

            manager.EditReview(review);

            return RedirectToAction("Index");
        }

        private void UpdateTopicAndName(int reviewId)
        {
            FinalThesisReview review = manager.GetReview(reviewId);
            SubmissionThesis submission = manager.GetSubmissionForThesisId(review.FinalThesis.FinalThesisId);

            ViewBag.thesisTopic = submission.ThesisTopic;
            ViewBag.name = review.FinalThesis.Participant.Name;
            ViewBag.surname = review.FinalThesis.Participant.Surname;
        }

        private List<bool> GetReviewFieldsValidation(IFormCollection form)
        {
            List<bool> fieldsValidation = new List<bool>();
            fieldsValidation.Add(DataValidator.FieldContentIsValid(form["TitleCompability"], maxLength: 127));
            fieldsValidation.Add(DataValidator.FieldContentIsValid(form["ThesisStructureComment"], maxLength: 127));
            fieldsValidation.Add(DataValidator.FieldContentIsValid(form["NewProblem"], maxLength: 127));
            fieldsValidation.Add(DataValidator.FieldContentIsValid(form["SourcesUse"], maxLength: 127));
            fieldsValidation.Add(DataValidator.FieldContentIsValid(form["FormalWorkSide"], maxLength: 255));
            fieldsValidation.Add(DataValidator.FieldContentIsValid(form["WayToUse"], maxLength: 255));
            fieldsValidation.Add(DataValidator.FieldContentIsValid(form["SubstantiveThesisGrade"], maxLength: 2047));
            fieldsValidation.Add(DataValidator.FieldContentIsValid(form["ThesisGrade"], maxLength: 15));
            fieldsValidation.Add(DataValidator.DateIsValid(form["FormDate"]));

            return fieldsValidation;
        }

        public IActionResult Preview(int id)
        {
            if (id != 0)
            {
                FinalThesisReview finalThesisReview = manager.GetReview(id);
                SubmissionThesis submission = manager.GetSubmissionForThesisId(id);
                ViewBag.topic = submission.ThesisTopic;
                return View(finalThesisReview);
            }
            return View();
        }
    }
}
