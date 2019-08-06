using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microting.eForm.Infrastructure.Constants;
using Microting.eForm.Infrastructure.Data.Entities;
using NUnit.Framework;

namespace eFormSDK.Tests
{
    
    [TestFixture]
    public class AnswerValuesUTest : DbTestFixture
    {
        [Test]
        public void AnswerValues_Create_DoesCreate()
        {
            //Arrange
            
             Random rnd = new Random();

             bool randomBool = rnd.Next(0, 2) > 0;

             sites site = new sites();
             site.Name = Guid.NewGuid().ToString();
             site.MicrotingUid = rnd.Next(1, 255);
             site.Create(DbContext);
             
             sites siteForUnit = new sites();
             siteForUnit.Name = Guid.NewGuid().ToString();
             siteForUnit.MicrotingUid = rnd.Next(1, 255);
             siteForUnit.Create(DbContext);
             
             units unit = new units();
             unit.CustomerNo = rnd.Next(1, 255);
             unit.MicrotingUid = rnd.Next(1, 255);
             unit.OtpCode = rnd.Next(1, 255);
             unit.SiteId = siteForUnit.Id;
             unit.Create(DbContext);
             
             languages language = new languages();
             language.Description = Guid.NewGuid().ToString();
             language.Name = Guid.NewGuid().ToString();
             language.Create(DbContext);
             
             question_sets questionSet = new question_sets();
             questionSet.Name = Guid.NewGuid().ToString();
             questionSet.Share = randomBool;
             questionSet.HasChild = randomBool;
             questionSet.PosiblyDeployed = randomBool;
             questionSet.Create(DbContext);
             
             survey_configurations surveyConfiguration = new survey_configurations();
             surveyConfiguration.Name = Guid.NewGuid().ToString();
             surveyConfiguration.Start = DateTime.Now;
             surveyConfiguration.Stop = DateTime.Now;
             surveyConfiguration.TimeOut = rnd.Next(1, 255);
             surveyConfiguration.TimeToLive = rnd.Next(1, 255);
             surveyConfiguration.Create(DbContext);
             
             answers answer = new answers();
             answer.AnswerDuration = rnd.Next(1, 255);
             answer.FinishedAt = rnd.Next(1, 255);
             answer.LanguageId = language.Id;
             answer.QuestionSetId = questionSet.Id;
             answer.SiteId = site.Id;
             answer.TimeZone = Guid.NewGuid().ToString();
             answer.UnitId = unit.Id;
             answer.UtcAdjusted = randomBool;
             answer.QuestionSetId = questionSet.Id;
             answer.SurveyConfigurationId = surveyConfiguration.Id;
             answer.Create(DbContext);
             
             question_sets questionSetForQuestion = new question_sets();
             questionSetForQuestion.Name = Guid.NewGuid().ToString();
             questionSetForQuestion.Share = randomBool;
             questionSetForQuestion.HasChild = randomBool;
             questionSetForQuestion.PosiblyDeployed = randomBool;
             questionSetForQuestion.Create(DbContext);
             
             questions question = new questions();
             question.Image = randomBool;
             question.Maximum = rnd.Next(1, 255);
             question.Minimum = rnd.Next(1, 255);
             question.Prioritised = randomBool;
             question.Type = Guid.NewGuid().ToString();
             question.FontSize = Guid.NewGuid().ToString();
             question.ImagePosition = Guid.NewGuid().ToString();
             question.MaxDuration = rnd.Next(1, 255);
             question.MinDuration = rnd.Next(1, 255);
             question.QuestionIndex = rnd.Next(1, 255);
             question.QuestionType = Guid.NewGuid().ToString();
             question.RefId = rnd.Next(1, 255);
             question.ValidDisplay = randomBool;
             question.BackButtonEnabled = randomBool;
             question.ContinuousQuestionId = rnd.Next(1, 255);
             question.QuestionSetId = questionSetForQuestion.Id;
             question.Create(DbContext);
             
             options option = new options();
             option.Question = question;
             option.Weight = rnd.Next(1, 255);
             option.OptionsIndex = rnd.Next(1, 255);
             option.QuestionId = question.Id;
             option.WeightValue = rnd.Next(1, 255);
             option.ContinuousOptionId = rnd.Next(1, 255);
             option.Create(DbContext);
             
             questions questionForAnswerValue = new questions();
             questionForAnswerValue.Image = randomBool;
             questionForAnswerValue.Maximum = rnd.Next(1, 255);
             questionForAnswerValue.Minimum = rnd.Next(1, 255);
             questionForAnswerValue.Prioritised = randomBool;
             questionForAnswerValue.Type = Guid.NewGuid().ToString();
             questionForAnswerValue.FontSize = Guid.NewGuid().ToString();
             questionForAnswerValue.ImagePosition = Guid.NewGuid().ToString();
             questionForAnswerValue.MaxDuration = rnd.Next(1, 255);
             questionForAnswerValue.MinDuration = rnd.Next(1, 255);
             questionForAnswerValue.QuestionIndex = rnd.Next(1, 255);
             questionForAnswerValue.QuestionType = Guid.NewGuid().ToString();
             questionForAnswerValue.RefId = rnd.Next(1, 255);
             questionForAnswerValue.ValidDisplay = randomBool;
             questionForAnswerValue.BackButtonEnabled = randomBool;
             questionForAnswerValue.ContinuousQuestionId = rnd.Next(1, 255);
             questionForAnswerValue.QuestionSetId = questionSetForQuestion.Id;
             questionForAnswerValue.Create(DbContext);
            
             answer_values answerValue = new answer_values();
             answerValue.Value = rnd.Next(1, 255);
             answerValue.AnswerId = answer.Id;
             answerValue.OptionsId = option.Id;
             answerValue.QuestionId = question.Id;
             
             //Act
             
             answerValue.Create(DbContext);
             
             List<answer_values> answerValues = DbContext.answer_values.AsNoTracking().ToList();
             List<answer_value_versions> answerValueVersions = DbContext.answer_value_versions.AsNoTracking().ToList();
            
             //Assert
            
             Assert.NotNull(answerValues);                                                             
             Assert.NotNull(answerValueVersions);                                                             

             Assert.AreEqual(1,answerValues.Count());  
             Assert.AreEqual(1,answerValueVersions.Count());  
            
             Assert.AreEqual(answerValue.CreatedAt.ToString(), answerValues[0].CreatedAt.ToString());                                  
             Assert.AreEqual(answerValue.Version, answerValues[0].Version);                                      
             Assert.AreEqual(answerValue.UpdatedAt.ToString(), answerValues[0].UpdatedAt.ToString());                                  
             Assert.AreEqual(answerValues[0].WorkflowState, Constants.WorkflowStates.Created);
             Assert.AreEqual(answerValue.Value, answerValues[0].Value);
             Assert.AreEqual(answerValue.Id, answerValues[0].Id);
             Assert.AreEqual(answer.Id, answerValue.AnswerId);
             Assert.AreEqual(option.Id, answerValue.OptionsId);
             Assert.AreEqual(question.Id, answerValue.QuestionId);
             
             //Versions
             Assert.AreEqual(answerValue.CreatedAt.ToString(), answerValueVersions[0].CreatedAt.ToString());                                  
             Assert.AreEqual(1, answerValueVersions[0].Version);                                      
             Assert.AreEqual(answerValue.UpdatedAt.ToString(), answerValueVersions[0].UpdatedAt.ToString());                                  
             Assert.AreEqual(answerValueVersions[0].WorkflowState, Constants.WorkflowStates.Created);
             Assert.AreEqual(answerValue.Id, answerValueVersions[0].AnswerValueId);
             Assert.AreEqual(answerValue.Value, answerValueVersions[0].Value);
             Assert.AreEqual(answer.Id, answerValueVersions[0].AnswerId);
             Assert.AreEqual(option.Id, answerValueVersions[0].OptionsId);
             Assert.AreEqual(question.Id, answerValueVersions[0].QuestionId);
        }

        [Test]
        public void AnswerValues_Update_DoesUpdate()
        {
            //Arrange
            
             Random rnd = new Random();

             bool randomBool = rnd.Next(0, 2) > 0;

             sites site = new sites();
             site.Name = Guid.NewGuid().ToString();
             site.MicrotingUid = rnd.Next(1, 255);
             site.Create(DbContext);
             
             sites siteForUnit = new sites();
             siteForUnit.Name = Guid.NewGuid().ToString();
             siteForUnit.MicrotingUid = rnd.Next(1, 255);
             siteForUnit.Create(DbContext);
             
             units unit = new units();
             unit.CustomerNo = rnd.Next(1, 255);
             unit.MicrotingUid = rnd.Next(1, 255);
             unit.OtpCode = rnd.Next(1, 255);
             unit.SiteId = siteForUnit.Id;
             unit.Create(DbContext);
             
             languages language = new languages();
             language.Description = Guid.NewGuid().ToString();
             language.Name = Guid.NewGuid().ToString();
             language.Create(DbContext);
             
             question_sets questionSet = new question_sets();
             questionSet.Name = Guid.NewGuid().ToString();
             questionSet.Share = randomBool;
             questionSet.HasChild = randomBool;
             questionSet.PosiblyDeployed = randomBool;
             questionSet.Create(DbContext);
             
             survey_configurations surveyConfiguration = new survey_configurations();
             surveyConfiguration.Name = Guid.NewGuid().ToString();
             surveyConfiguration.Start = DateTime.Now;
             surveyConfiguration.Stop = DateTime.Now;
             surveyConfiguration.TimeOut = rnd.Next(1, 255);
             surveyConfiguration.TimeToLive = rnd.Next(1, 255);
             surveyConfiguration.Create(DbContext);
             
             answers answer = new answers();
             answer.AnswerDuration = rnd.Next(1, 255);
             answer.FinishedAt = rnd.Next(1, 255);
             answer.LanguageId = language.Id;
             answer.SiteId = site.Id;
             answer.TimeZone = Guid.NewGuid().ToString();
             answer.UnitId = unit.Id;
             answer.UtcAdjusted = randomBool;
             answer.QuestionSetId = questionSet.Id;
             answer.SurveyConfigurationId = surveyConfiguration.Id;
             answer.Create(DbContext);
             
             question_sets questionSetForQuestion = new question_sets();
             questionSet.Name = Guid.NewGuid().ToString();
             questionSet.Share = randomBool;
             questionSet.HasChild = randomBool;
             questionSet.PosiblyDeployed = randomBool;
             questionSetForQuestion.Create(DbContext);
             
             questions question = new questions();
             question.Image = randomBool;
             question.Maximum = rnd.Next(1, 255);
             question.Minimum = rnd.Next(1, 255);
             question.Prioritised = randomBool;
             question.Type = Guid.NewGuid().ToString();
             question.FontSize = Guid.NewGuid().ToString();
             question.ImagePosition = Guid.NewGuid().ToString();
             question.MaxDuration = rnd.Next(1, 255);
             question.MinDuration = rnd.Next(1, 255);
             question.QuestionIndex = rnd.Next(1, 255);
             question.QuestionType = Guid.NewGuid().ToString();
             question.RefId = rnd.Next(1, 255);
             question.ValidDisplay = randomBool;
             question.BackButtonEnabled = randomBool;
             question.ContinuousQuestionId = rnd.Next(1, 255);
             question.QuestionSetId = questionSetForQuestion.Id;
             question.Create(DbContext);
             
             options option = new options();
             option.Question = question;
             option.Weight = rnd.Next(1, 255);
             option.OptionsIndex = rnd.Next(1, 255);
             option.QuestionId = question.Id;
             option.WeightValue = rnd.Next(1, 255);
             option.ContinuousOptionId = rnd.Next(1, 255);
             option.Create(DbContext);
             
             questions questionForAnswerValue = new questions();
             questionForAnswerValue.Image = randomBool;
             questionForAnswerValue.Maximum = rnd.Next(1, 255);
             questionForAnswerValue.Minimum = rnd.Next(1, 255);
             questionForAnswerValue.Prioritised = randomBool;
             questionForAnswerValue.Type = Guid.NewGuid().ToString();
             questionForAnswerValue.FontSize = Guid.NewGuid().ToString();
             questionForAnswerValue.ImagePosition = Guid.NewGuid().ToString();
             questionForAnswerValue.MaxDuration = rnd.Next(1, 255);
             questionForAnswerValue.MinDuration = rnd.Next(1, 255);
             questionForAnswerValue.QuestionIndex = rnd.Next(1, 255);
             questionForAnswerValue.QuestionType = Guid.NewGuid().ToString();
             questionForAnswerValue.RefId = rnd.Next(1, 255);
             questionForAnswerValue.ValidDisplay = randomBool;
             questionForAnswerValue.BackButtonEnabled = randomBool;
             questionForAnswerValue.ContinuousQuestionId = rnd.Next(1, 255);
             questionForAnswerValue.QuestionSetId = questionSetForQuestion.Id;
             questionForAnswerValue.Create(DbContext);
            
             answer_values answerValue = new answer_values();
             answerValue.Value = rnd.Next(1, 255);
             answerValue.AnswerId = answer.Id;
             answerValue.OptionsId = option.Id;
             answerValue.QuestionId = question.Id;
             answerValue.Create(DbContext);
             
             //Act
             
             DateTime? oldUpdatedAt = answerValue.UpdatedAt;
             int oldValue = answerValue.Value;

             answerValue.Value = rnd.Next(1, 255);
             
             answerValue.Update(DbContext);
             
             
             List<answer_values> answerValues = DbContext.answer_values.AsNoTracking().ToList();
             List<answer_value_versions> answerValueVersions = DbContext.answer_value_versions.AsNoTracking().ToList();
            
             //Assert
            
             Assert.NotNull(answerValues);                                                             
             Assert.NotNull(answerValueVersions);                                                             

             Assert.AreEqual(1,answerValues.Count());  
             Assert.AreEqual(2,answerValueVersions.Count());  
            
             Assert.AreEqual(answerValue.CreatedAt.ToString(), answerValues[0].CreatedAt.ToString());                                  
             Assert.AreEqual(answerValue.Version, answerValues[0].Version);                                      
             Assert.AreEqual(answerValue.UpdatedAt.ToString(), answerValues[0].UpdatedAt.ToString());                                  
             Assert.AreEqual(answerValues[0].WorkflowState, Constants.WorkflowStates.Created);
             Assert.AreEqual(answerValue.Value, answerValues[0].Value);
             Assert.AreEqual(answerValue.Id, answerValues[0].Id);
             Assert.AreEqual(answerValue.AnswerId, answer.Id);
             Assert.AreEqual(answerValue.OptionsId, option.Id);
             Assert.AreEqual(answerValue.QuestionId, question.Id);
             
             //Old Version
             Assert.AreEqual(answerValue.CreatedAt.ToString(), answerValueVersions[0].CreatedAt.ToString());                                  
             Assert.AreEqual(1, answerValueVersions[0].Version);                                      
             Assert.AreEqual(oldUpdatedAt.ToString(), answerValueVersions[0].UpdatedAt.ToString());                                  
             Assert.AreEqual(answerValueVersions[0].WorkflowState, Constants.WorkflowStates.Created);
             Assert.AreEqual(answerValue.Id, answerValueVersions[0].AnswerValueId);
             Assert.AreEqual(oldValue, answerValueVersions[0].Value);
             Assert.AreEqual(answer.Id, answerValueVersions[0].AnswerId);
             Assert.AreEqual(option.Id, answerValueVersions[0].OptionsId);
             Assert.AreEqual(question.Id, answerValueVersions[0].QuestionId);
             
             //New Version
             Assert.AreEqual(answerValue.CreatedAt.ToString(), answerValueVersions[1].CreatedAt.ToString());                                  
             Assert.AreEqual(2, answerValueVersions[1].Version);                                      
             Assert.AreEqual(answerValue.UpdatedAt.ToString(), answerValueVersions[1].UpdatedAt.ToString());                                  
             Assert.AreEqual(answerValueVersions[1].WorkflowState, Constants.WorkflowStates.Created);
             Assert.AreEqual(answerValue.Id, answerValueVersions[1].AnswerValueId);
             Assert.AreEqual(answerValue.Value, answerValueVersions[1].Value);
             Assert.AreEqual(answer.Id, answerValueVersions[1].AnswerId);
             Assert.AreEqual(option.Id, answerValueVersions[1].OptionsId);
             Assert.AreEqual(question.Id, answerValueVersions[1].QuestionId);
        }

        [Test]
        public void AnswerValues_Delete_DoesSetWorkflowStateToRemoved()
        {
            //Arrange
            
             Random rnd = new Random();

             bool randomBool = rnd.Next(0, 2) > 0;

             sites site = new sites();
             site.Name = Guid.NewGuid().ToString();
             site.MicrotingUid = rnd.Next(1, 255);
             site.Create(DbContext);
             
             sites siteForUnit = new sites();
             siteForUnit.Name = Guid.NewGuid().ToString();
             siteForUnit.MicrotingUid = rnd.Next(1, 255);
             siteForUnit.Create(DbContext);
             
             units unit = new units();
             unit.CustomerNo = rnd.Next(1, 255);
             unit.MicrotingUid = rnd.Next(1, 255);
             unit.OtpCode = rnd.Next(1, 255);
             unit.SiteId = siteForUnit.Id;
             unit.Create(DbContext);
             
             languages language = new languages();
             language.Description = Guid.NewGuid().ToString();
             language.Name = Guid.NewGuid().ToString();
             language.Create(DbContext);
             
             question_sets questionSet = new question_sets();
             questionSet.Name = Guid.NewGuid().ToString();
             questionSet.Share = randomBool;
             questionSet.HasChild = randomBool;
             questionSet.PosiblyDeployed = randomBool;
             questionSet.Create(DbContext);
             
             survey_configurations surveyConfiguration = new survey_configurations();
             surveyConfiguration.Name = Guid.NewGuid().ToString();
             surveyConfiguration.Start = DateTime.Now;
             surveyConfiguration.Stop = DateTime.Now;
             surveyConfiguration.TimeOut = rnd.Next(1, 255);
             surveyConfiguration.TimeToLive = rnd.Next(1, 255);
             surveyConfiguration.Create(DbContext);
             
             answers answer = new answers();
             answer.AnswerDuration = rnd.Next(1, 255);
             answer.FinishedAt = rnd.Next(1, 255);
             answer.LanguageId = language.Id;
             answer.SiteId = site.Id;
             answer.SurveyConfiguration = surveyConfiguration;
             answer.TimeZone = Guid.NewGuid().ToString();
             answer.UnitId = unit.Id;
             answer.UtcAdjusted = randomBool;
             answer.QuestionSetId = questionSet.Id;
             answer.SurveyConfigurationId = surveyConfiguration.Id;
             answer.Create(DbContext);
             
             question_sets questionSetForQuestion = new question_sets();
             questionSet.Name = Guid.NewGuid().ToString();
             questionSet.Share = randomBool;
             questionSet.HasChild = randomBool;
             questionSet.PosiblyDeployed = randomBool;
             questionSetForQuestion.Create(DbContext);
             
             questions question = new questions();
             question.Image = randomBool;
             question.Maximum = rnd.Next(1, 255);
             question.Minimum = rnd.Next(1, 255);
             question.Prioritised = randomBool;
             question.Type = Guid.NewGuid().ToString();
             question.FontSize = Guid.NewGuid().ToString();
             question.ImagePosition = Guid.NewGuid().ToString();
             question.MaxDuration = rnd.Next(1, 255);
             question.MinDuration = rnd.Next(1, 255);
             question.QuestionIndex = rnd.Next(1, 255);
             question.QuestionType = Guid.NewGuid().ToString();
             question.RefId = rnd.Next(1, 255);
             question.ValidDisplay = randomBool;
             question.BackButtonEnabled = randomBool;
             question.ContinuousQuestionId = rnd.Next(1, 255);
             question.QuestionSetId = questionSetForQuestion.Id;
             question.Create(DbContext);
             
             options option = new options();
             option.Weight = rnd.Next(1, 255);
             option.OptionsIndex = rnd.Next(1, 255);
             option.QuestionId = question.Id;
             option.WeightValue = rnd.Next(1, 255);
             option.ContinuousOptionId = rnd.Next(1, 255);
             option.Create(DbContext);
             
             questions questionForAnswerValue = new questions();
             questionForAnswerValue.Image = randomBool;
             questionForAnswerValue.Maximum = rnd.Next(1, 255);
             questionForAnswerValue.Minimum = rnd.Next(1, 255);
             questionForAnswerValue.Prioritised = randomBool;
             questionForAnswerValue.Type = Guid.NewGuid().ToString();
             questionForAnswerValue.FontSize = Guid.NewGuid().ToString();
             questionForAnswerValue.ImagePosition = Guid.NewGuid().ToString();
             questionForAnswerValue.MaxDuration = rnd.Next(1, 255);
             questionForAnswerValue.MinDuration = rnd.Next(1, 255);
             questionForAnswerValue.QuestionIndex = rnd.Next(1, 255);
             questionForAnswerValue.QuestionType = Guid.NewGuid().ToString();
             questionForAnswerValue.RefId = rnd.Next(1, 255);
             questionForAnswerValue.ValidDisplay = randomBool;
             questionForAnswerValue.BackButtonEnabled = randomBool;
             questionForAnswerValue.ContinuousQuestionId = rnd.Next(1, 255);
             questionForAnswerValue.QuestionSetId = questionSetForQuestion.Id;
             questionForAnswerValue.Create(DbContext);
            
             answer_values answerValue = new answer_values();
             answerValue.Value = rnd.Next(1, 255);
             answerValue.AnswerId = answer.Id;
             answerValue.OptionsId = option.Id;
             answerValue.QuestionId = question.Id;
             answerValue.Create(DbContext);
             
             //Act
             
             DateTime? oldUpdatedAt = answerValue.UpdatedAt;
             
             answerValue.Delete(DbContext);
             
             
             List<answer_values> answerValues = DbContext.answer_values.AsNoTracking().ToList();
             List<answer_value_versions> answerValueVersions = DbContext.answer_value_versions.AsNoTracking().ToList();
            
             //Assert
            
             Assert.NotNull(answerValues);                                                             
             Assert.NotNull(answerValueVersions);                                                             

             Assert.AreEqual(1,answerValues.Count());  
             Assert.AreEqual(2,answerValueVersions.Count());  
            
             Assert.AreEqual(answerValue.CreatedAt.ToString(), answerValues[0].CreatedAt.ToString());                                  
             Assert.AreEqual(answerValue.Version, answerValues[0].Version);                                      
             Assert.AreEqual(answerValue.UpdatedAt.ToString(), answerValues[0].UpdatedAt.ToString());                                  
             Assert.AreEqual(answerValues[0].WorkflowState, Constants.WorkflowStates.Removed);
             Assert.AreEqual(answerValue.Value, answerValues[0].Value);
             Assert.AreEqual(answerValue.Id, answerValues[0].Id);
             Assert.AreEqual(answerValue.AnswerId, answer.Id);
             Assert.AreEqual(answerValue.OptionsId, option.Id);
             Assert.AreEqual(answerValue.QuestionId, question.Id);
             
             //Old Version
             Assert.AreEqual(answerValue.CreatedAt.ToString(), answerValueVersions[0].CreatedAt.ToString());                                  
             Assert.AreEqual(1, answerValueVersions[0].Version);                                      
             Assert.AreEqual(oldUpdatedAt.ToString(), answerValueVersions[0].UpdatedAt.ToString());                                  
             Assert.AreEqual(answerValueVersions[0].WorkflowState, Constants.WorkflowStates.Created);
             Assert.AreEqual(answerValue.Id, answerValueVersions[0].AnswerValueId);
             Assert.AreEqual(answerValue.Value, answerValueVersions[0].Value);
             Assert.AreEqual(answer.Id, answerValueVersions[0].AnswerId);
             Assert.AreEqual(option.Id, answerValueVersions[0].OptionsId);
             Assert.AreEqual(question.Id, answerValueVersions[0].QuestionId);
             
             //New Version
             Assert.AreEqual(answerValue.CreatedAt.ToString(), answerValueVersions[1].CreatedAt.ToString());                                  
             Assert.AreEqual(2, answerValueVersions[1].Version);                                      
             Assert.AreEqual(answerValue.UpdatedAt.ToString(), answerValueVersions[1].UpdatedAt.ToString());                                  
             Assert.AreEqual(answerValueVersions[1].WorkflowState, Constants.WorkflowStates.Removed);
             Assert.AreEqual(answerValue.Id, answerValueVersions[1].AnswerValueId);
             Assert.AreEqual(answerValue.Value, answerValueVersions[1].Value);
             Assert.AreEqual(answer.Id, answerValueVersions[1].AnswerId);
             Assert.AreEqual(option.Id, answerValueVersions[1].OptionsId);
             Assert.AreEqual(question.Id, answerValueVersions[1].QuestionId);
        }
    }
}