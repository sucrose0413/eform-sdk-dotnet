/*
The MIT License (MIT)

Copyright (c) 2007 - 2019 microting

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Linq;

namespace Microting.eForm.Infrastructure.Data.Entities
{
    public partial class question_sets : BaseEntity
    {
        public string Name { get; set; }
        
        public bool HasChild { get; set; }
        
        public bool PosiblyDeployed { get; set; }
        
        public int ParentId { get; set; }
        
        public bool Share { get; set; }


        public void Create(MicrotingDbAnySql dbContext)
        {
            WorkflowState = Constants.Constants.WorkflowStates.Created;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
            Version = 1;

            dbContext.question_sets.Add(this);
            dbContext.SaveChanges();

            dbContext.question_set_versions.Add(MapVersions(this));
            dbContext.SaveChanges();
        }

        public void Update(MicrotingDbAnySql dbContext)
        {
            question_sets questionSet = dbContext.question_sets.FirstOrDefault(x => x.Id == Id);

            if (questionSet == null)
            {
                throw new NullReferenceException($"Could not find question set with Id: {Id}");
            }

            questionSet.Name = Name;
            questionSet.Share = Share;
            questionSet.HasChild = HasChild;
            questionSet.PosiblyDeployed = PosiblyDeployed;

            if (dbContext.ChangeTracker.HasChanges())
            {
                UpdatedAt = DateTime.Now;
                Version += 1;

                dbContext.question_set_versions.Add(MapVersions(questionSet));
                dbContext.SaveChanges();
              
            }
        }

        public void Delete(MicrotingDbAnySql dbContext)
        {
            question_sets questionSet = dbContext.question_sets.FirstOrDefault(x => x.Id == Id);

            if (questionSet == null)
            {
                throw new NullReferenceException($"Could not find question set with Id: {Id}");
            }

            questionSet.WorkflowState = Constants.Constants.WorkflowStates.Removed;
            
            if (dbContext.ChangeTracker.HasChanges())
            {
                UpdatedAt = DateTime.Now;
                Version += 1;

                dbContext.question_set_versions.Add(MapVersions(questionSet));
                dbContext.SaveChanges();
              
            }
        }
        
        private question_set_versions MapVersions(question_sets questionSet)
        {
            question_set_versions questionSetVersions = new question_set_versions();

            questionSetVersions.QuestionSetId = questionSet.Id;
            questionSetVersions.Name = questionSet.Name;
            questionSetVersions.Share = questionSet.Share;
            questionSetVersions.HasChild = questionSet.HasChild;
            questionSetVersions.PossiblyDeployed = questionSet.PosiblyDeployed;
            questionSetVersions.Version = questionSet.Version;
            questionSetVersions.CreatedAt = questionSet.CreatedAt;
            questionSetVersions.UpdatedAt = questionSet.UpdatedAt;
            questionSetVersions.WorkflowState = questionSet.WorkflowState;

            
            return questionSetVersions;
        }
    }
}