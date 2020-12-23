﻿/*
The MIT License (MIT)

Copyright (c) 2007 - 2020 Microting A/S

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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microting.eForm.Infrastructure;
using Microting.eForm.Infrastructure.Constants;
using Microting.eForm.Infrastructure.Data.Entities;

namespace Microting.eForm.Helpers
{
    public class TestHelpers
    {
        public MicrotingDbContext dbContext;

        private Language language;
//        private string returnXML;
//        private string returnJSON;
        public TestHelpers()
        {

            string connectionString;

//            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
//            {
//                ConnectionString = @"data source=(LocalDb)\SharedInstance;Initial catalog=eformsdk-tests;Integrated Security=True";
//            }
//            else
//            {
                connectionString = @"Server = localhost; port = 3306; Database = eformsdk-tests; user = root; password = 'secretpassword'; Convert Zero Datetime = true;";
//            }

            dbContext = GetContext(connectionString);
            language = dbContext.Languages.Single(x => x.Name == "Danish");
        }
        private MicrotingDbContext GetContext(string connectionStr)
        {

            DbContextOptionsBuilder dbContextOptionsBuilder = new DbContextOptionsBuilder();

            // if (connectionStr.ToLower().Contains("convert zero datetime"))
            // {
                dbContextOptionsBuilder.UseMySql(connectionStr);
            // }
            // else
            // {
                // dbContextOptionsBuilder.UseSqlServer(connectionStr);
            // }
            dbContextOptionsBuilder.UseLazyLoadingProxies(true);
            return new MicrotingDbContext(dbContextOptionsBuilder.Options);

        }
        #region helperMethods
        public async Task<Worker> CreateWorker(string email, string firstName, string lastName, int microtingUId)
        {
            Worker worker = new Worker
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                MicrotingUid = microtingUId,
                WorkflowState = Constants.WorkflowStates.Created,
                Version = 69
            };
            dbContext.Workers.Add(worker);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            return worker;
        }
        public async Task<Site> CreateSite(string name, int microtingUId)
        {

            Site site = new Site
            {
                Name = name,
                MicrotingUid = microtingUId,
                UpdatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Version = 64,
                WorkflowState = Constants.WorkflowStates.Created
            };
            dbContext.Sites.Add(site);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            return site;
        }
        public async Task<Unit> CreateUnit(int microtingUId, int otpCode, Site site, int customerNo)
        {

            Unit unit = new Unit
            {
                MicrotingUid = microtingUId,
                OtpCode = otpCode,
                Site = site,
                SiteId = site.Id,
                CreatedAt = DateTime.UtcNow,
                CustomerNo = customerNo,
                UpdatedAt = DateTime.UtcNow,
                Version = 9,
                WorkflowState = Constants.WorkflowStates.Created
            };

            dbContext.Units.Add(unit);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            return unit;
        }
        public async Task<SiteWorker> CreateSiteWorker(int microtingUId, Site site, Worker worker)
        {
            SiteWorker siteWorker = new SiteWorker
            {
                CreatedAt = DateTime.UtcNow,
                MicrotingUid = microtingUId,
                UpdatedAt = DateTime.UtcNow,
                Version = 63,
                Site = site,
                SiteId = site.Id,
                Worker = worker,
                WorkerId = worker.Id,
                WorkflowState = Constants.WorkflowStates.Created
            };

            dbContext.SiteWorkers.Add(siteWorker);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            return siteWorker;
        }
        public async Task<CheckList> CreateTemplate(DateTime cl_ca, DateTime cl_ua, string label, string description, string caseType, string folderName, int displayIndex, int repeated)
        {
            CheckList cl1 = new CheckList
            {
                CreatedAt = cl_ca,
                UpdatedAt = cl_ua,
                // Label = label,
                // Description = description,
                WorkflowState = Constants.WorkflowStates.Created,
                CaseType = caseType,
                FolderName = folderName,
                DisplayIndex = displayIndex,
                Repeated = repeated,
                ParentId = 0,
            };

            dbContext.CheckLists.Add(cl1);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            CheckLisTranslation checkLisTranslation = new CheckLisTranslation()
            {
                LanguageId = language.Id,
                Text = label,
                Description = description,
                CheckListId = cl1.Id
            };
            await checkLisTranslation.Create(dbContext);
            return cl1;
        }
        public async Task<CheckList> CreateSubTemplate(string label, string description, string caseType, int displayIndex, int repeated, CheckList parentId)
        {
            CheckList cl2 = new CheckList
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                // Label = label,
                // Description = description,
                WorkflowState = Constants.WorkflowStates.Created,
                CaseType = caseType,
                DisplayIndex = displayIndex,
                Repeated = repeated,
                ParentId = parentId.Id
            };

            dbContext.CheckLists.Add(cl2);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            CheckLisTranslation checkLisTranslation = new CheckLisTranslation()
            {
                LanguageId = language.Id,
                Text = label,
                Description = description,
                CheckListId = cl2.Id
            };
            await checkLisTranslation.Create(dbContext);
            return cl2;
        }
        public async Task<Field> CreateField(short? barcodeEnabled, string barcodeType, CheckList checkList, string color, string custom, int? decimalCount, string defaultValue, string description, int? displayIndex, short? dummy, FieldType ft, short? geolocationEnabled, short? geolocationForced, short? geolocationHidden, short? isNum, string label, short? mandatory, int maxLength, string maxValue, string minValue, short? multi, short? optional, string queryType, short? readOnly, short? selected, short? split, short? stopOnSave, string unitName, int version)
        {

            Field f = new Field
            {
                FieldTypeId = ft.Id,
                BarcodeEnabled = barcodeEnabled,
                BarcodeType = barcodeType,
                CheckListId = checkList.Id,
                Color = color,
                CreatedAt = DateTime.UtcNow,
                Custom = custom,
                DecimalCount = decimalCount,
                DefaultValue = defaultValue,
                // Description = description,
                DisplayIndex = displayIndex,
                Dummy = dummy,
                GeolocationEnabled = geolocationEnabled,
                GeolocationForced = geolocationForced,
                GeolocationHidden = geolocationHidden,
                IsNum = isNum,
                // Label = label,
                Mandatory = mandatory,
                MaxLength = maxLength,
                MaxValue = maxValue,
                MinValue = minValue,
                Multi = multi,
                Optional = optional,
                QueryType = queryType,
                ReadOnly = readOnly,
                Selected = selected,
                Split = split,
                StopOnSave = stopOnSave,
                UnitName = unitName,
                UpdatedAt = DateTime.UtcNow,
                Version = version,
                WorkflowState = Constants.WorkflowStates.Created
            };


            dbContext.Fields.Add(f);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            FieldTranslation fieldTranslation = new FieldTranslation()
            {
                LanguageId = language.Id,
                Text = label,
                Description = description,
                FieldId = f.Id
            };
            await fieldTranslation.Create(dbContext);
            Thread.Sleep(2000);

            return f;
        }
        public async Task<Case> CreateCase(string caseUId, CheckList checkList, DateTime created_at, string custom, DateTime done_at, Worker doneByUserId, int microtingCheckId, int microtingUId, Site site, int? status, string caseType, Unit unit, DateTime updated_at, int version, Worker worker, string WorkFlowState)
        {

            Case aCase = new Case
            {
                CaseUid = caseUId,
                CheckList = checkList,
                CheckListId = checkList.Id,
                CreatedAt = created_at,
                Custom = custom,
                DoneAt = done_at,
                WorkerId = worker.Id,
                MicrotingCheckUid = microtingCheckId,
                MicrotingUid = microtingUId,
                Site = site,
                SiteId = site.Id,
                Status = status,
                Type = caseType,
                Unit = unit,
                UnitId = unit.Id,
                UpdatedAt = updated_at,
                Version = version,
                Worker = worker,
                WorkflowState = WorkFlowState
            };

            dbContext.Cases.Add(aCase);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            return aCase;
        }
        public async Task<FieldValue> CreateFieldValue(Case aCase, CheckList checkList, Field f, int? ud_id, int? userId, string value, int? version, Worker worker)
        {
            FieldValue fv = new FieldValue
            {
                CaseId = aCase.Id,
                CheckList = checkList,
                CheckListId = checkList.Id,
                CreatedAt = DateTime.UtcNow,
                Date = DateTime.UtcNow,
                DoneAt = DateTime.UtcNow,
                Field = f,
                FieldId = f.Id,
                UpdatedAt = DateTime.UtcNow,
                WorkerId = userId,
                Value = value,
                Version = version,
                Worker = worker,
                WorkflowState = Constants.WorkflowStates.Created
            };
            if (ud_id != null)
            {
                fv.UploadedDataId = ud_id;
            }

            dbContext.FieldValues.Add(fv);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return fv;
        }
        public async Task<CheckListValue> CreateCheckListValue(Case aCase, CheckList checkList, string status, int? userId, int? version)
        {
            CheckListValue CLV = new CheckListValue
            {
                CaseId = aCase.Id,
                CheckListId = checkList.Id,
                CreatedAt = DateTime.UtcNow,
                Status = status,
                UpdatedAt = DateTime.UtcNow,
                UserId = userId,
                Version = version,
                WorkflowState = Constants.WorkflowStates.Created
            };

            dbContext.CheckListValues.Add(CLV);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return CLV;

        }
        public async Task<UploadedData> CreateUploadedData(string checkSum, string currentFile, string extension, string fileLocation, string fileName, short? local, Worker worker, string uploaderType, int version, bool createPhysicalFile)
        {
            UploadedData UD = new UploadedData
            {
                Checksum = checkSum,
                CreatedAt = DateTime.UtcNow,
                CurrentFile = currentFile,
                ExpirationDate = DateTime.UtcNow.AddYears(1),
                Extension = extension,
                FileLocation = fileLocation,
                FileName = fileName,
                Local = local,
                UpdatedAt = DateTime.UtcNow,
                UploaderId = worker.Id,
                UploaderType = uploaderType,
                Version = version,
                WorkflowState = Constants.WorkflowStates.Created
            };


            dbContext.UploadedDatas.Add(UD);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);


            string path = System.IO.Path.Combine(fileLocation, fileName);

            if (createPhysicalFile)
            {
                System.IO.Directory.CreateDirectory(fileLocation);
                if (!System.IO.File.Exists(path))
                {
                    using (System.IO.FileStream fs = System.IO.File.Create(path))
                    {
                        for (byte i = 0; i < 100; i++)
                        {
                            fs.WriteByte(i);
                        }
                    }
                }
            }
            return UD;
        }
        public async Task<EntityGroup> CreateEntityGroup(string microtingUId, string name, string entityType, string workflowState)
        {

            var lists =  dbContext.EntityGroups.Where(x => x.MicrotingUid == microtingUId).ToList();

            if (lists.Count == 0)
            {
                EntityGroup eG = new EntityGroup
                {
                    CreatedAt = DateTime.UtcNow,
                    MicrotingUid = microtingUId,
                    Name = name,
                    Type = entityType,
                    UpdatedAt = DateTime.UtcNow,
                    Version = 1,
                    WorkflowState = workflowState
                };

                //eG.Id = xxx;

                dbContext.EntityGroups.Add(eG);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
                return eG;
            } else {
                throw new ArgumentException("microtingUId already exists: " + microtingUId);
            }

        }
        public async Task<EntityItem> CreateEntityItem(string description, int displayIndex, int entityGroupId, string entityItemUId, string microtingUId, string name, short? synced, int version, string workflowState)
        {
            EntityItem eI = new EntityItem
            {
                CreatedAt = DateTime.UtcNow,
                Description = description,
                DisplayIndex = displayIndex,
                EntityGroupId = entityGroupId,
                EntityItemUid = entityItemUId,
                MicrotingUid = microtingUId,
                Name = name,
                Synced = synced,
                UpdatedAt = DateTime.UtcNow,
                Version = version,
                WorkflowState = workflowState
            };

            dbContext.EntityItems.Add(eI);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            return eI;
        }
        public async Task <Tag> CreateTag(string name, string workflowState, int version)
        {
            Tag tag = new Tag {Name = name, WorkflowState = workflowState, Version = version};
            await tag.Create(dbContext).ConfigureAwait(false);

            return tag;
        }
        public async Task<CheckListSite> CreateCheckListSite(CheckList checklist, DateTime createdAt,
            Site site, DateTime updatedAt, int version, string workflowState, int microting_uid)
        {
            CheckListSite cls = new CheckListSite
            {
                CheckList = checklist,
                CreatedAt = createdAt,
                Site = site,
                UpdatedAt = updatedAt,
                Version = version,
                WorkflowState = workflowState,
                MicrotingUid = microting_uid
            };
            await cls.Create(dbContext).ConfigureAwait(false);
//            DbContext.check_list_sites.Add(cls);
//            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return cls;
        }

        public async Task<int> GetRandomInt()
        {
            await Task.Run(() => { }); // TODO FIX ME
            Random random = new Random();
            int i = random.Next(0, int.MaxValue);
            return i;
        }
        #endregion


    }
}
