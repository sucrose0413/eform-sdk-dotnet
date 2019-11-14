﻿/*
The MIT License (MIT)

Copyright (c) 2007 - 2019 Microting A/S

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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microting.eForm.Dto;
using Microting.eForm.Infrastructure.Data.Entities;
using Microting.eForm.Infrastructure.Extensions;
using Microting.eForm.Infrastructure.Models;
using Microting.eForm.Infrastructure.Models.reply;
//using eFormSqlController.Migrations;

namespace Microting.eForm.Infrastructure
{
    public class SqlController : LogWriter
    {
        #region var

        private readonly string connectionStr;
        private Log log;
        private readonly Tools t = new Tools();
        private List<Holder> converter = null;
        private readonly bool isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        private int logLimit = 0;
        #endregion

        #region con
        public SqlController(string connectionString)
        {
            connectionStr = connectionString;          

            #region migrate if needed
            try
            {
                using (var db = GetContext())
                {
                    WriteDebugConsoleLogEntry(new LogEntry(2, "SqlController.SqlController", "db.Database.Migrate() called"));
                    db.Database.Migrate();
                    
                    WriteDebugConsoleLogEntry(new LogEntry(2, "SqlController.SqlController", "db.Database.EnsureCreated() called"));
                    db.Database.EnsureCreated();

                   var match = db.settings.Count();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
//                Console.WriteLine(ex.InnerException.Message);
                throw ex;
               // MigrateDb();
            }
            #endregion

            //region set default for settings if needed
            if (SettingCheckAll().Result.Count > 0)
            {
                bool result = SettingCreateDefaults().Result;
            }
            
            logLimit = int.Parse(SettingRead(Settings.logLimit).Result);
        }

        private MicrotingDbAnySql GetContext()
        {

            DbContextOptionsBuilder dbContextOptionsBuilder = new DbContextOptionsBuilder();
//            WriteDebugConsoleLogEntry(new LogEntry(2, "SqlController", "GetContext called")); // This is done to avoid infinite loop

            if (connectionStr.ToLower().Contains("convert zero datetime"))
            {
                dbContextOptionsBuilder.UseMySql(connectionStr);
            }
            else
            {
                dbContextOptionsBuilder.UseSqlServer(connectionStr);
            }
            dbContextOptionsBuilder.UseLazyLoadingProxies(true);
            return new MicrotingDbAnySql(dbContextOptionsBuilder.Options);

        }
        #endregion

        #region public
        #region public template
        
       //TODO
        public async Task<int> TemplateCreate(MainElement mainElement)
        {
            try
            {
                var result = await EformCreateDb(mainElement);
                
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("TemplatCreate failed", ex);
            }
        }

        //TODO
        public async Task<MainElement> TemplateRead(int templateId)
        {
            try
            {
                using (var db = GetContext())
                {
                    MainElement mainElement = null;
                    GetConverter();

                    check_lists mainCl = await db.check_lists.SingleOrDefaultAsync(x => x.Id == templateId && (x.ParentId == null || x.ParentId == 0));

                    if (mainCl == null)
                        return null;

                    mainElement = new MainElement(mainCl.Id, mainCl.Label, t.Int(mainCl.DisplayIndex), mainCl.FolderName, t.Int(mainCl.Repeated), DateTime.Now, DateTime.Now.AddDays(2), "da",
                        t.Bool(mainCl.MultiApproval), t.Bool(mainCl.FastNavigation), t.Bool(mainCl.DownloadEntities), t.Bool(mainCl.ManualSync), mainCl.CaseType, "", "", t.Bool(mainCl.QuickSyncEnabled), new List<Element>(), mainCl.Color);

                    //getting elements
                    List<check_lists> lst = db.check_lists.Where(x => x.ParentId == templateId).ToList();
                    foreach (check_lists cl in lst)
                    {
                        mainElement.ElementList.Add(await GetElement(cl.Id));
                    }

                    //return
                    return mainElement;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("TemplatRead failed", ex);
            }
        }

        //TODO
        public async Task<Template_Dto> TemplateItemRead(int templateId)
        {
            try
            {
                using (var db = GetContext())
                {
                    check_lists checkList = await db.check_lists.SingleOrDefaultAsync(x => x.Id == templateId);

                    if (checkList == null)
                        return null;

                    List<SiteName_Dto> sites = new List<SiteName_Dto>();
                    foreach (check_list_sites check_list_site in checkList.CheckListSites.Where(x => x.WorkflowState != Constants.Constants.WorkflowStates.Removed).ToList())
                    {
                        SiteName_Dto site = new SiteName_Dto((int)check_list_site.Site.MicrotingUid, check_list_site.Site.Name, check_list_site.Site.CreatedAt, check_list_site.Site.UpdatedAt);
                        sites.Add(site);
                    }
                    bool hasCases = db.cases.Where(x => x.CheckListId == checkList.Id).AsQueryable().Count() != 0;
                    
                    #region load fields
                    Field_Dto fd1 = null;
                    Field_Dto fd2 = null;
                    Field_Dto fd3 = null;
                    Field_Dto fd4 = null;
                    Field_Dto fd5 = null;
                    Field_Dto fd6 = null;
                    Field_Dto fd7 = null;
                    Field_Dto fd8 = null;
                    Field_Dto fd9 = null;
                    Field_Dto fd10 = null;

                    fields f1 = await db.fields.SingleOrDefaultAsync(x => x.Id == checkList.Field1);
                    if (f1 != null)
                        fd1 = new Field_Dto(f1.Id, f1.Label, f1.Description, (int)f1.FieldTypeId, f1.FieldType.FieldType, (int)f1.CheckListId);

                    fields f2 = await db.fields.SingleOrDefaultAsync(x => x.Id == checkList.Field2);
                    if (f2 != null)
                        fd2 = new Field_Dto(f2.Id, f2.Label, f2.Description, (int)f2.FieldTypeId, f2.FieldType.FieldType, (int)f2.CheckListId);

                    fields f3 = await db.fields.SingleOrDefaultAsync(x => x.Id == checkList.Field3);
                    if (f3 != null)
                        fd3 = new Field_Dto(f3.Id, f3.Label, f3.Description, (int)f3.FieldTypeId, f3.FieldType.FieldType, (int)f3.CheckListId);

                    fields f4 = await db.fields.SingleOrDefaultAsync(x => x.Id == checkList.Field4);
                    if (f4 != null)
                        fd4 = new Field_Dto(f4.Id, f4.Label, f4.Description, (int)f4.FieldTypeId, f4.FieldType.FieldType, (int)f4.CheckListId);

                    fields f5 = await db.fields.SingleOrDefaultAsync(x => x.Id == checkList.Field5);
                    if (f5 != null)
                        fd5 = new Field_Dto(f5.Id, f5.Label, f5.Description, (int)f5.FieldTypeId, f5.FieldType.FieldType, (int)f5.CheckListId);

                    fields f6 = await db.fields.SingleOrDefaultAsync(x => x.Id == checkList.Field6);
                    if (f6 != null)
                        fd6 = new Field_Dto(f6.Id, f6.Label, f6.Description, (int)f6.FieldTypeId, f6.FieldType.FieldType, (int)f6.CheckListId);

                    fields f7 = await db.fields.SingleOrDefaultAsync(x => x.Id == checkList.Field7);
                    if (f7 != null)
                        fd7 = new Field_Dto(f7.Id, f7.Label, f7.Description, (int)f7.FieldTypeId, f7.FieldType.FieldType, (int)f7.CheckListId);

                    fields f8 = await db.fields.SingleOrDefaultAsync(x => x.Id == checkList.Field8);
                    if (f8 != null)
                        fd8 = new Field_Dto(f8.Id, f8.Label, f8.Description, (int)f8.FieldTypeId, f8.FieldType.FieldType, (int)f8.CheckListId);

                    fields f9 = await db.fields.SingleOrDefaultAsync(x => x.Id == checkList.Field9);
                    if (f9 != null)
                        fd9 = new Field_Dto(f9.Id, f9.Label, f9.Description, (int)f9.FieldTypeId, f9.FieldType.FieldType, (int)f9.CheckListId);

                    fields f10 = await db.fields.SingleOrDefaultAsync(x => x.Id == checkList.Field10);
                    if (f10 != null)
                        fd10 = new Field_Dto(f10.Id, f10.Label, f10.Description, (int)f10.FieldTypeId, f10.FieldType.FieldType, (int)f10.CheckListId);
                    #endregion

                    #region loadtags
                    List<taggings> matches = checkList.Taggings.ToList();
                    List<KeyValuePair<int, string>> check_list_tags = new List<KeyValuePair<int, string>>();
                    foreach (taggings tagging in matches)
                    {
                        KeyValuePair<int, string> kvp = new KeyValuePair<int, string>((int)tagging.TagId, tagging.Tag.Name);
                        check_list_tags.Add(kvp);
                    }
                    #endregion

                    Template_Dto templateDto = new Template_Dto(checkList.Id, checkList.CreatedAt, checkList.UpdatedAt,
                        checkList.Label, checkList.Description, (int) checkList.Repeated, checkList.FolderName,
                        checkList.WorkflowState, sites, hasCases, checkList.DisplayIndex, fd1, fd2, fd3, fd4, fd5, fd6,
                        fd7, fd8, fd9, fd10, check_list_tags, checkList.JasperExportEnabled, checkList.DocxExportEnabled);
                    return templateDto;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(t.GetMethodName("SQLController") + " failed", ex);
            }
        }

        //TODO
        public async Task<List<Template_Dto>> TemplateItemReadAll(bool includeRemoved, string siteWorkflowState, string searchKey, bool descendingSort, string sortParameter, List<int> tagIds)
        {
            string methodName = t.GetMethodName("SQLController");
            await log.LogStandard(methodName, "called");
            await log.LogVariable(methodName, nameof(includeRemoved), includeRemoved);
            await log.LogVariable(methodName, nameof(searchKey), searchKey);
            await log.LogVariable(methodName, nameof(descendingSort), descendingSort);
            await log.LogVariable(methodName, nameof(sortParameter), sortParameter);
            await log.LogVariable(methodName, nameof(tagIds), tagIds.ToString());
            try
            {
                List<Template_Dto> templateList = new List<Template_Dto>();

                using (var db = GetContext())
                {
                    List<check_lists> matches = null;
                    IQueryable<check_lists> sub_query = db.check_lists.Where(x => x.ParentId == null);

                    if (!includeRemoved)
                        sub_query = sub_query.Where(x => x.WorkflowState == Constants.Constants.WorkflowStates.Created);

                    if (!string.IsNullOrEmpty(searchKey))
                    {
                        sub_query = sub_query.Where(x => x.Label.Contains(searchKey) || x.Description.Contains(searchKey));
                    }

                    if (tagIds.Count > 0)
                    {
                        List<int?> check_list_ids = db.taggings.Where(x => tagIds.Contains((int)x.TagId)).Select(x => x.CheckListId).ToList();
                        if (check_list_ids != null)
                        {
                            sub_query = sub_query.Where(x => check_list_ids.Contains(x.Id));
                        }
                        
                    }


                    switch (sortParameter)
                    {
                        case Constants.Constants.eFormSortParameters.Label:
                            if (descendingSort)
                                sub_query = sub_query.OrderByDescending(x => x.Label);
                            else
                                sub_query = sub_query.OrderBy(x => x.Label);
                            break;
                        case Constants.Constants.eFormSortParameters.Description:
                            if (descendingSort)
                                sub_query = sub_query.OrderByDescending(x => x.Description);
                            else
                                sub_query = sub_query.OrderBy(x => x.Description);
                            break;
                        case Constants.Constants.eFormSortParameters.CreatedAt:
                            if (descendingSort)
                                sub_query = sub_query.OrderByDescending(x => x.CreatedAt);
                            else
                                sub_query = sub_query.OrderBy(x => x.CreatedAt);
                            break;
                        case Constants.Constants.eFormSortParameters.Tags:
                            if (descendingSort)
                                sub_query = sub_query.OrderByDescending(x => x.CreatedAt);
                            else
                                sub_query = sub_query.OrderBy(x => x.CreatedAt);
                            break;
                        default:
                            if (descendingSort)
                                sub_query = sub_query.OrderByDescending(x => x.Id);
                            else
                                sub_query = sub_query.OrderBy(x => x.Id);
                            break;
                    }

                    matches = sub_query.ToList();

                    foreach (check_lists checkList in matches)
                    {
                        List<SiteName_Dto> sites = new List<SiteName_Dto>();
                        List<check_list_sites> check_list_sites = null;

                        if (siteWorkflowState == Constants.Constants.WorkflowStates.Removed)
                            check_list_sites = checkList.CheckListSites.Where(x => x.WorkflowState == Constants.Constants.WorkflowStates.Removed).ToList();
                        else
                            check_list_sites = checkList.CheckListSites.Where(x => x.WorkflowState != Constants.Constants.WorkflowStates.Removed).ToList();

                        foreach (check_list_sites check_list_site in check_list_sites)
                        {
                            try
                            {
                                SiteName_Dto site = new SiteName_Dto((int)check_list_site.Site.MicrotingUid, check_list_site.Site.Name, check_list_site.Site.CreatedAt, check_list_site.Site.UpdatedAt);
                                sites.Add(site);
                            } catch (Exception innerEx)
                            {
                                await log.LogException(methodName, "could not add the site to the sites for site.Id : " + check_list_site.Site.Id.ToString() + " and got exception : " + innerEx.Message, innerEx, false);
                                throw new Exception("Error adding site to sites for site.Id : " + check_list_site.Site.Id.ToString(), innerEx);
                            }
                            
                        }

                        bool hasCases = db.cases.Where(x => x.CheckListId == checkList.Id).AsQueryable().Count() != 0;
                        
                        #region loadtags
                        List<taggings> tagging_matches = checkList.Taggings.Where(x => x.WorkflowState != Constants.Constants.WorkflowStates.Removed).ToList();
                        List<KeyValuePair<int, string>> check_list_tags = new List<KeyValuePair<int, string>>();
                        foreach (taggings tagging in tagging_matches)
                        {
                            KeyValuePair<int, string> kvp = new KeyValuePair<int, string>((int)tagging.TagId, tagging.Tag.Name);
                            check_list_tags.Add(kvp);
                        }
                        #endregion
                        try
                        {
                            Template_Dto templateDto = new Template_Dto(checkList.Id, checkList.CreatedAt, checkList.UpdatedAt, checkList.Label, checkList.Description, (int)checkList.Repeated, checkList.FolderName, checkList.WorkflowState, sites, hasCases, checkList.DisplayIndex, check_list_tags);
                            templateList.Add(templateDto);
                        }
                        catch (Exception innerEx)
                        {
                            await log.LogException(methodName, "could not add the templateDto to the templateList for templateId : " + checkList.Id.ToString() + " and got exception : " + innerEx.Message, innerEx, false);
                            throw new Exception("Error adding template to templateList for templateId : " + checkList.Id.ToString(), innerEx);
                        }
                    }
                    return templateList;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("TemplateSimpleReadAll failed", ex);
            }


        }

        //TODO
        public async Task<List<Field_Dto>> TemplateFieldReadAll(int templateId)
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                using (var db = GetContext())
                {
                    MainElement mainElement = await TemplateRead(templateId);
                    List<Field_Dto> fieldLst = new List<Field_Dto>();

                    foreach (DataItem dataItem in mainElement.DataItemGetAll())
                    {
                        fields field = await db.fields.SingleAsync(x => x.Id == dataItem.Id);
                        Field_Dto fieldDto = new Field_Dto(field.Id, field.Label, field.Description, (int)field.FieldTypeId, field.FieldType.FieldType, (int)field.CheckListId);
                        if (field.ParentFieldId != null)
                        {
                            fieldDto.ParentName = db.fields.Where(x => x.Id == field.ParentFieldId).First().Label;
                        }
                        else
                        {
                            fieldDto.ParentName = field.CheckList.Label;
                        }
                        fieldLst.Add(fieldDto);
                    }


                    return fieldLst;
                }
            }
            catch (Exception ex)
            {
                //logger.LogException(methodName + " failed", ex, true);
                throw new Exception(methodName + " failed", ex);
            }
        }

        //TODO
        public async Task<bool> TemplateDisplayIndexChange(int templateId, int newDisplayIndex)
        {
            try
            {
                using (var db = GetContext())
                {
                    check_lists checkList = await db.check_lists.SingleOrDefaultAsync(x => x.Id == templateId);

                    if (checkList == null)
                        return false;

                    checkList.DisplayIndex = newDisplayIndex;

                    await checkList.Update(db);

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(t.GetMethodName("SQLController") + " failed", ex);
            }
        }

        //TODO
        public async Task<bool> TemplateUpdateFieldIdsForColumns(int templateId, int? fieldId1, int? fieldId2, int? fieldId3, int? fieldId4, int? fieldId5, int? fieldId6, int? fieldId7, int? fieldId8, int? fieldId9, int? fieldId10)
        {
            try
            {
                using (var db = GetContext())
                {
                    check_lists checkList = await db.check_lists.SingleOrDefaultAsync(x => x.Id == templateId);

                    if (checkList == null)
                        return false;

                    checkList.Field1 = fieldId1;
                    checkList.Field2 = fieldId2;
                    checkList.Field3 = fieldId3;
                    checkList.Field4 = fieldId4;
                    checkList.Field5 = fieldId5;
                    checkList.Field6 = fieldId6;
                    checkList.Field7 = fieldId7;
                    checkList.Field8 = fieldId8;
                    checkList.Field9 = fieldId9;
                    checkList.Field10 = fieldId10;
                    
                    await checkList.Update(db);

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(t.GetMethodName("SQLController") + " failed", ex);
            }
        }

        /// <summary>
        /// Deletes Template from DB with given templateId
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> TemplateDelete(int templateId)
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                using (var db = GetContext())
                {
                    //logger.LogEverything(methodName + " called");
                    //logger.LogEverything("siteName:" + siteName + " / userFirstName:" + userFirstName + " / userLastName:" + userLastName);

                    check_lists check_list = await db.check_lists.SingleAsync(x => x.Id == templateId);

                    if (check_list != null)
                    {
                        await check_list.Delete((db));

                        return true;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                //logger.LogException(methodName + " failed", ex, true);
                throw new Exception(methodName + " failed", ex);
            }
        }

        //TODO
        public async Task<bool> TemplateSetTags(int templateId, List<int> tagIds)
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                using (var db = GetContext())
                {
                    //logger.LogEverything(methodName + " called");
                    //logger.LogEverything("siteName:" + siteName + " / userFirstName:" + userFirstName + " / userLastName:" + userLastName);

                    check_lists checkList = await db.check_lists.SingleAsync(x => x.Id == templateId);

                    if (checkList != null)
                    {
                        // Delete all not wanted taggings first
                        List<taggings> clTaggings = checkList.Taggings.Where(x => !tagIds.Contains((int)x.TagId)).ToList();
//                        int index = 0;
                        foreach (taggings tagging in clTaggings)
                        {
                            taggings currentTagging = await db.taggings.SingleAsync(x => x.Id == tagging.Id);
                            if (currentTagging != null)
                            {
                                await currentTagging.Delete(db);
                            }
                        }

                        // set all new taggings
                        foreach (int Id in tagIds)
                        {
                            tags tag = await db.tags.SingleAsync(x => x.Id == Id);
                            if (tag != null)
                            {
                                taggings currentTagging = await db.taggings.SingleOrDefaultAsync(x => x.TagId == tag.Id && x.CheckListId == templateId);

                                if (currentTagging == null)
                                {
                                    taggings tagging = new taggings();
                                    tagging.CheckListId = templateId;
                                    tagging.TagId = tag.Id;

                                    await tagging.Create(db);
                                } else {
                                    if (currentTagging.WorkflowState != Constants.Constants.WorkflowStates.Created)
                                    {
                                        currentTagging.WorkflowState = Constants.Constants.WorkflowStates.Created;
                                        await currentTagging.Update(db);
                                    }
                                }                                
                            }
                        }

                        return true;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                //logger.LogException(methodName + " failed", ex, true);
                throw new Exception(methodName + " failed", ex);
            }
        }

        public async Task SetJasperExportEnabled(int eFormId, bool isEnabled)
        {
            string methodName = t.GetMethodName("SQLController"); 
            using (var db = GetContext())
            {
                check_lists checkList = await db.check_lists.SingleOrDefaultAsync(x => x.Id == eFormId);
                if (checkList != null)
                {
                    checkList.JasperExportEnabled = isEnabled;
                    await checkList.Update(db);
                }
            }
        }

        public async Task SetDocxExportEnabled(int eFormId, bool isEnabled)
        {
            string methodName = t.GetMethodName("SQLController"); 
            using (var db = GetContext())
            {
                check_lists checkList = await db.check_lists.SingleOrDefaultAsync(x => x.Id == eFormId);
                if (checkList != null)
                {
                    checkList.DocxExportEnabled = isEnabled;
                    await checkList.Update(db);
                }
            }
        }
        
        #endregion

        #region public (pre)case

        //TODO
        public async Task CheckListSitesCreate(int checkListId, int siteUId, int microtingUId)
        {
            try
            {
                using (var db = GetContext())
                {
                    int siteId = db.sites.SingleAsync(x => x.MicrotingUid == siteUId).Result.Id;

                    check_list_sites cLS = new check_list_sites();
                    cLS.CheckListId = checkListId;
                    cLS.LastCheckId = 0;
                    cLS.MicrotingUid = microtingUId;
                    cLS.SiteId = siteId;
                    await cLS.Create(db);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CheckListSitesCreate failed", ex);
            }
        }

        //TODO
#pragma warning disable 1998
        public async Task<List<int>> CheckListSitesRead(int templateId, int microtingUid, string workflowState)
        {
            try
            {
                using (var db = GetContext())
                {
                    sites site = await db.sites.SingleAsync(x => x.MicrotingUid == microtingUid);
                    IQueryable<check_list_sites> sub_query = db.check_list_sites.Where(x => x.SiteId == site.Id && x.CheckListId == templateId);
                    if (workflowState == Constants.Constants.WorkflowStates.NotRemoved)
                        sub_query = sub_query.Where(x => x.WorkflowState != Constants.Constants.WorkflowStates.Removed);

                    return sub_query.Select(x => x.MicrotingUid).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CheckListSitesRead failed", ex);
            }
        }
#pragma warning restore 1998


        /// <summary>
        /// Creates a case in DB with given parameters siteUId, microtingUId, microtingCheckId, caseUid, custom and createdAt
        /// After creation the method returns the ID of the newly created case
        /// </summary>
        /// <param name="checkListId"></param>
        /// <param name="siteUId"></param>
        /// <param name="microtingUId"></param>
        /// <param name="microtingCheckId"></param>
        /// <param name="caseUId"></param>
        /// <param name="custom"></param>
        /// <param name="createdAt"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> CaseCreate(int checkListId, int siteUId, int? microtingUId, int? microtingCheckId, string caseUId, string custom, DateTime createdAt)
        {
            string methodName = t.GetMethodName("SQLController");
            await log.LogStandard(methodName, "called");
            try
            {
                using (var db = GetContext())
                {
                    string caseType = db.check_lists.SingleAsync(x => x.Id == checkListId).Result.CaseType;
                    await log.LogStandard(methodName, $"caseType is {caseType}");
                    int siteId = db.sites.SingleAsync(x => x.MicrotingUid == siteUId).Result.Id;
                    await log.LogStandard(methodName, $"siteId is {siteId}");

                    cases aCase = null;
                    // Lets see if we have an existing case with the same parameters in the db first.
                    // This is to handle none gracefull shutdowns.                
                    aCase = await db.cases.SingleOrDefaultAsync(x => x.MicrotingUid == microtingUId && x.MicrotingCheckUid == microtingCheckId);
                    await log.LogStandard(methodName, $"aCase found based on MicrotingUid == {microtingUId} and MicrotingCheckUid == {microtingCheckId}");

                    if (aCase == null)
                    {
                        aCase = new cases();
                        aCase.Status = 66;
                        aCase.Type = caseType;
                        aCase.CheckListId = checkListId;
                        aCase.MicrotingUid = microtingUId;
                        aCase.MicrotingCheckUid = microtingCheckId;
                        aCase.CaseUid = caseUId;
                        aCase.SiteId = siteId;

                        aCase.Custom = custom;

                        await aCase.Create(db);
                    }
                    else
                    {
                        aCase.Status = 66;
                        aCase.Type = caseType;
                        aCase.CheckListId = checkListId;
                        aCase.MicrotingUid = microtingUId;
                        aCase.MicrotingCheckUid = microtingCheckId;
                        aCase.CaseUid = caseUId;
                        aCase.SiteId = siteId;
                        aCase.Custom = custom;
                        await aCase.Update(db);
                    }
                    await log.LogStandard(methodName, $"aCase is created in db");

                    return aCase.Id;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CaseCreate failed", ex);
            }
        }

        /// <summary>
        /// Reads case from DB with given microtingUId
        /// </summary>
        /// <param name="microtingUId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int?> CaseReadLastCheckIdByMicrotingUId(int microtingUId)
        {
            try
            {
                using (var db = GetContext())
                {
                    check_list_sites match = await db.check_list_sites.SingleOrDefaultAsync(x => x.MicrotingUid == microtingUId);
                    if (match == null)
                        return null;
                    else
                        return match.LastCheckId;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CaseReadByMuuId failed", ex);
            }
        }

        //TODO
        public async Task CaseUpdateRetreived(int microtingUId)
        {
            try
            {
                using (var db = GetContext())
                {
                    cases match = await db.cases.SingleOrDefaultAsync(x => x.MicrotingUid == microtingUId);

                    if (match != null)
                    {
                        match.Status = 77;
                        await match.Update(db);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CaseUpdateRetrived failed", ex);
            }
        }

        //TODO
        public async Task CaseUpdateCompleted(int microtingUId, int microtingCheckId, DateTime doneAt, int workerMicrotingUId, int unitMicrotingUid)
        {
            try
            {
                using (var db = GetContext())
                {
                    cases caseStd = await db.cases.SingleOrDefaultAsync(x => x.MicrotingUid == microtingUId && x.MicrotingCheckUid == microtingCheckId);

                    if (caseStd == null)
                        caseStd = await db.cases.SingleAsync(x => x.MicrotingUid == microtingUId);

                    int userId = db.workers.SingleAsync(x => x.MicrotingUid == workerMicrotingUId).Result.Id;
                    int unitId = db.units.SingleAsync(x => x.MicrotingUid == unitMicrotingUid).Result.Id;

                    caseStd.Status = 100;
                    caseStd.DoneAt = doneAt;
                    caseStd.WorkerId = userId;
                    caseStd.UnitId = unitId;
                    caseStd.MicrotingCheckUid = microtingCheckId;
                    #region - update "check_list_sites" if needed
                    check_list_sites match = await db.check_list_sites.SingleOrDefaultAsync(x => x.MicrotingUid == microtingUId);
                    if (match != null)
                    {
                        match.LastCheckId = microtingCheckId;
                        await match.Update(db);
                    }
                    #endregion

                    await caseStd.Update(db);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CaseUpdateCompleted failed", ex);
            }
        }

        //TODO
        public async Task CaseRetract(int microtingUId, int microtingCheckId)
        {
            try
            {
                using (var db = GetContext())
                {
                    cases match = await db.cases.SingleAsync(x => x.MicrotingUid == microtingUId && x.MicrotingCheckUid == microtingCheckId);

                    match.WorkflowState = Constants.Constants.WorkflowStates.Retracted;
                    await match.Update(db);
                }   
            }
            catch (Exception ex)
            {
                throw new Exception("CaseRetract failed", ex);
            }
        }

        /// <summary>
        /// Deletes Case from DB with given microtingUId
        /// </summary>
        /// <param name="microtingUId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> CaseDelete(int microtingUId)
        {
            try
            {
                using (var db = GetContext())
                {
                    cases aCase = await db.cases.SingleOrDefaultAsync(x => x.MicrotingUid == microtingUId);
                    if (aCase != null)
                    {
                        if (aCase.WorkflowState != Constants.Constants.WorkflowStates.Retracted && aCase.WorkflowState != Constants.Constants.WorkflowStates.Removed)
                        {
                            await aCase.Delete((db));
                        }
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CaseDelete failed", ex);
            }
        }

        //TODO
        public async Task<bool> CaseDeleteResult(int caseId)
        {
            try
            {
                using (var db = GetContext())
                {
                    cases aCase = await db.cases.SingleOrDefaultAsync(x => x.Id == caseId);

                    if (aCase != null)
                    {
                        await aCase.Delete(db);

                        return true;
                    } else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CaseDelete failed", ex);
            }
        }

        //TODO
        public async Task CaseDeleteReversed(int microtingUId)
        {
            try
            {
                using (var db = GetContext())
                {
                    check_list_sites checkListSites = await db.check_list_sites.SingleAsync(x => x.MicrotingUid == microtingUId);

                    await checkListSites.Delete(db);

                }
            }
            catch (Exception ex)
            {
                throw new Exception("CaseDelete failed", ex);
            }
        }
        #endregion

        #region public "reply"
        #region check
        
        //TODO
        public async Task<List<int>> ChecksCreate(Response response, string xmlString, int xmlIndex)
        {
            List<int> uploadedDataIds = new List<int>();
            try
            {
                using (var db = GetContext())
                {
                    int elementId;
                    int userUId = int.Parse(response.Checks[xmlIndex].WorkerId);
                    int userId = db.workers.SingleAsync(x => x.MicrotingUid == userUId).Result.Id;
                    List<string> elements = t.LocateList(xmlString, "<ElementList>", "</ElementList>");
                    List<Field_Dto> TemplatFieldLst = null;
                    cases responseCase = null;
                    List<int?> case_fields = new List<int?>();
                    List<int> fieldTypeIds = db.field_types.Where(x => x.FieldType == Constants.Constants.FieldTypes.Picture || x.FieldType == Constants.Constants.FieldTypes.Signature || x.FieldType == Constants.Constants.FieldTypes.Audio).Select(x => x.Id).ToList();

                    try //if a reversed case, case needs to be created
                    {
                        check_list_sites cLS = await db.check_list_sites.SingleAsync(x => x.MicrotingUid == int.Parse(response.Value));
                        int caseId = await CaseCreate((int)cLS.CheckListId, (int)cLS.Site.MicrotingUid, int.Parse(response.Value), response.Checks[xmlIndex].Id, "ReversedCase", "", DateTime.Now);
                        responseCase = await db.cases.SingleAsync(x => x.Id == caseId);
                    }
                    catch //already created case Id retrived
                    {
                        responseCase = await db.cases.SingleAsync(x => x.MicrotingUid == int.Parse(response.Value));
                    }

                    check_lists cl = responseCase.CheckList;

                    case_fields.Add(cl.Field1);
                    case_fields.Add(cl.Field2);
                    case_fields.Add(cl.Field3);
                    case_fields.Add(cl.Field4);
                    case_fields.Add(cl.Field5);
                    case_fields.Add(cl.Field6);
                    case_fields.Add(cl.Field7);
                    case_fields.Add(cl.Field8);
                    case_fields.Add(cl.Field9);
                    case_fields.Add(cl.Field10);
                    //cl.field_1

                    TemplatFieldLst = await TemplateFieldReadAll((int)responseCase.CheckListId);

                    foreach (string elementStr in elements)
                    {
                        #region foreach element

                        int cl_id = int.Parse(t.Locate(elementStr, "<Id>", "</"));
                        int case_id = responseCase.Id;

                        check_list_values clv = null;
                        clv = await db.check_list_values.SingleOrDefaultAsync(x => x.CheckListId == cl_id && x.CaseId == case_id);

                        if (clv == null)
                        {
                            clv = new check_list_values();
                            clv.CheckListId = int.Parse(t.Locate(elementStr, "<Id>", "</"));
                            clv.CaseId = responseCase.Id;
                            clv.Status = t.Locate(elementStr, "<Status>", "</");
                            clv.UserId = userId;

                            await clv.Create(db);
                        }

                        

                        #region foreach (string dataItemStr in dataItems)
                        elementId = clv.Id;
                        List<string> dataItems = t.LocateList(elementStr, "<DataItem>", "</DataItem>");

                        if (dataItems != null)
                        {
                            foreach (string dataItemStr in dataItems)
                            {


                                int field_id = int.Parse(t.Locate(dataItemStr, "<Id>", "</"));

                                fields f = await db.fields.SingleAsync(x => x.Id == field_id);
                                field_values fieldV = null;


                                if (!fieldTypeIds.Contains((int)f.FieldTypeId)) 
                                {
                                    fieldV = await db.field_values.SingleOrDefaultAsync(x => x.FieldId == field_id && x.CaseId == case_id && x.CheckListId == cl_id && x.WorkerId == userId);
                                }


                                if (fieldV == null)
                                {
                                    fieldV = new field_values();

                                    //= new field_values();

                                    #region if contains a file
                                    string urlXml = t.Locate(dataItemStr, "<URL>", "</URL>");
                                    if (urlXml != "" && urlXml != "none")
                                    {
                                        uploaded_data dU = null;
                                        string fileLocation = t.Locate(dataItemStr, "<URL>", "</");
                                        dU = new uploaded_data();
                                        dU.Extension = t.Locate(dataItemStr, "<Extension>", "</");
                                        dU.UploaderId = userId;
                                        dU.UploaderType = Constants.Constants.UploaderTypes.System;
                                        dU.Local = 0;
                                        dU.FileLocation = fileLocation;
                                        await dU.Create(db);
                                        fieldV.UploadedDataId = dU.Id;
                                        uploadedDataIds.Add(dU.Id);

                                    }
                                    #endregion
                                    #region fieldV.value = t.Locate(xml, "<Value>", "</");
                                    string extractedValue = t.Locate(dataItemStr, "<Value>", "</");

                                    if (extractedValue.Length > 8)
                                    {
                                        if (extractedValue.StartsWith(@"<![CDATA["))
                                        {
                                            extractedValue = extractedValue.Substring(9);
                                            extractedValue = extractedValue.Substring(0, extractedValue.Length - 3);
                                        }
                                    }

                                    if (f.FieldType.FieldType == Constants.Constants.FieldTypes.Number || 
                                        f.FieldType.FieldType == Constants.Constants.FieldTypes.NumberStepper)
                                    {
//                                        extractedValue = extractedValue.Replace(",", "|"); // commented as of 8. oct. 2019
                                        extractedValue = extractedValue.Replace(".", ",");
                                    }
                                    
                                    fieldV.Value = extractedValue;                                    
                                    fields _field = await db.fields.SingleOrDefaultAsync(x => x.Id == field_id);
                                    if (_field.FieldType.FieldType == Constants.Constants.FieldTypes.EntitySearch || _field.FieldType.FieldType == Constants.Constants.FieldTypes.EntitySelect)
                                    {
                                        if (!string.IsNullOrEmpty(extractedValue) && extractedValue != "null")
                                        {
                                            int Id = EntityItemRead(extractedValue).Result.Id;
                                            fieldV.Value = Id.ToString();
                                        }
                                    }
                                    
                                    #endregion
                                    //geo
                                    fieldV.Latitude = t.Locate(dataItemStr, "<Latitude>", "</");
                                    fieldV.Longitude = t.Locate(dataItemStr, "<Longitude>", "</");
                                    fieldV.Altitude = t.Locate(dataItemStr, "<Altitude>", "</");
                                    fieldV.Heading = t.Locate(dataItemStr, "<Heading>", "</");
                                    fieldV.Accuracy = t.Locate(dataItemStr, "<Accuracy>", "</");
                                    fieldV.Date = t.Date(t.Locate(dataItemStr, "<Date>", "</"));
                                    fieldV.CaseId = responseCase.Id;
                                    fieldV.FieldId = field_id;
                                    fieldV.WorkerId = userId;
                                    fieldV.CheckListId = clv.CheckListId;
                                    fieldV.DoneAt = t.Date(response.Checks[xmlIndex].Date);

                                    await fieldV.Create(db);

                                    #region update case field_values
                                    if (case_fields.Contains(fieldV.FieldId))
                                    {
                                        field_types field_type = db.fields.FirstAsync(x => x.Id == fieldV.FieldId).Result.FieldType;
                                        string new_value = fieldV.Value;

                                        if (field_type.FieldType == Constants.Constants.FieldTypes.EntitySearch || field_type.FieldType == Constants.Constants.FieldTypes.EntitySelect)
                                        {
                                            try
                                            {
                                                if (fieldV.Value != "" || fieldV.Value != null)
                                                {
                                                    int Id = int.Parse(fieldV.Value);
                                                    entity_items match = await db.entity_items.SingleOrDefaultAsync(x => x.Id == Id);
                                                    
                                                    if (match != null)
                                                    {
                                                        new_value = match.Name;
                                                    }

                                                }
                                            }
                                            catch { }
                                        }

                                        if (field_type.FieldType == "SingleSelect")
                                        {
                                            string key = fieldV.Value;
                                            string fullKey = t.Locate(fieldV.Field.KeyValuePairList, "<" + key + ">", "</" + key + ">");
                                            new_value = t.Locate(fullKey, "<key>", "</key>");
                                        }

                                        if (field_type.FieldType == "MultiSelect")
                                        {
                                            new_value = "";

                                            string keys = fieldV.Value;
                                            List<string> keyLst = keys.Split('|').ToList();

                                            foreach (string key in keyLst)
                                            {
                                                string fullKey = t.Locate(fieldV.Field.KeyValuePairList, "<" + key + ">", "</" + key + ">");
                                                if (new_value != "")
                                                {
                                                    new_value += "\n" + t.Locate(fullKey, "<key>", "</key>");
                                                }
                                                else
                                                {
                                                    new_value += t.Locate(fullKey, "<key>", "</key>");
                                                }
                                            }
                                        }


                                        int i = case_fields.IndexOf(fieldV.FieldId);
                                        switch (i)
                                        {
                                            case 0:
                                                responseCase.FieldValue1 = new_value;
                                                break;
                                            case 1:
                                                responseCase.FieldValue2 = new_value;
                                                break;
                                            case 2:
                                                responseCase.FieldValue3 = new_value;
                                                break;
                                            case 3:
                                                responseCase.FieldValue4 = new_value;
                                                break;
                                            case 4:
                                                responseCase.FieldValue5 = new_value;
                                                break;
                                            case 5:
                                                responseCase.FieldValue6 = new_value;
                                                break;
                                            case 6:
                                                responseCase.FieldValue7 = new_value;
                                                break;
                                            case 7:
                                                responseCase.FieldValue8 = new_value;
                                                break;
                                            case 8:
                                                responseCase.FieldValue9 = new_value;
                                                break;
                                            case 9:
                                                responseCase.FieldValue10 = new_value;
                                                break;
                                        }
                                        await responseCase.Update(db);
                                    }

                                    #endregion

                                    #region remove dataItem duplicate from TemplatDataItemLst
                                    int index = 0;
                                    foreach (var field in TemplatFieldLst)
                                    {
                                        if (fieldV.FieldId == field.Id)
                                        {
                                            TemplatFieldLst.RemoveAt(index);
                                            break;
                                        }

                                        index++;
                                    }
                                    #endregion
                                }
                            }
                        }
                        #endregion
                        #endregion
                    }

                    #region foreach (var field in TemplatFieldLst)
                    // We do this because even thought the user did not fill in information for a given field
                    // we need the field_value to be populated.
                    foreach (var field in TemplatFieldLst)
                    {
                        //field_values fieldV = new field_values();

                        field_values fieldV = null;

                        fieldV = await db.field_values.SingleOrDefaultAsync(x => x.FieldId == field.Id && x.CaseId == responseCase.Id && x.CheckListId == field.CheckListId && x.WorkerId == userId);

                        if (fieldV == null)
                        {
                            fieldV = new field_values();
                            fieldV.CreatedAt = DateTime.Now;
                            fieldV.UpdatedAt = DateTime.Now;

                            fieldV.Value = null;

                            //geo
                            fieldV.Latitude = null;
                            fieldV.Longitude = null;
                            fieldV.Altitude = null;
                            fieldV.Heading = null;
                            fieldV.Accuracy = null;
                            fieldV.Date = null;
                            //
                            fieldV.WorkflowState = Constants.Constants.WorkflowStates.Created;
                            fieldV.Version = 1;
                            fieldV.CaseId = responseCase.Id;
                            fieldV.FieldId = field.Id;
                            fieldV.WorkerId = userId;
                            fieldV.CheckListId = field.CheckListId;
                            fieldV.DoneAt = t.Date(response.Checks[xmlIndex].Date);

                            await fieldV.Create(db);
                        }

                        
                    }
                    #endregion
                }
                return uploadedDataIds;
            }
            catch (Exception ex)
            {
                throw new Exception("EformCheckCreateDb failed", ex);
            }
        }

        /// <summary>
        /// Updates Case Field Value in DB with given caseId
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> UpdateCaseFieldValue(int caseId)
        {
            try
            {
                using (var db = GetContext())
                {
                    cases match = await db.cases.SingleOrDefaultAsync(x => x.Id == caseId);


                    if (match != null)
                    {
                        List<int?> case_fields = new List<int?>();

                        check_lists cl = match.CheckList;
                        field_values fv = null;
                        #region field_value and field matching
                        fv = await db.field_values.SingleOrDefaultAsync(x => x.CaseId == caseId && x.FieldId == cl.Field1);
                        if (fv != null)
                        {
                            match.FieldValue1 = fv.Value;
                        }
                        fv = await db.field_values.SingleOrDefaultAsync(x => x.CaseId == caseId && x.FieldId == cl.Field2);
                        if (fv != null)
                        {
                            match.FieldValue2 = fv.Value;
                        }
                        fv = await db.field_values.SingleOrDefaultAsync(x => x.CaseId == caseId && x.FieldId == cl.Field3);
                        if (fv != null)
                        {
                            match.FieldValue3 = fv.Value;
                        }
                        fv = await db.field_values.SingleOrDefaultAsync(x => x.CaseId == caseId && x.FieldId == cl.Field4);
                        if (fv != null)
                        {
                            match.FieldValue4 = fv.Value;
                        }
                        fv = await db.field_values.SingleOrDefaultAsync(x => x.CaseId == caseId && x.FieldId == cl.Field5);
                        if (fv != null)
                        {
                            match.FieldValue5 = fv.Value;
                        }
                        fv = await db.field_values.SingleOrDefaultAsync(x => x.CaseId == caseId && x.FieldId == cl.Field6);
                        if (fv != null)
                        {
                            match.FieldValue6 = fv.Value;
                        }
                        fv = await db.field_values.SingleOrDefaultAsync(x => x.CaseId == caseId && x.FieldId == cl.Field7);
                        if (fv != null)
                        {
                            match.FieldValue7 = fv.Value;
                        }
                        fv = await db.field_values.SingleOrDefaultAsync(x => x.CaseId == caseId && x.FieldId == cl.Field8);
                        if (fv != null)
                        {
                            match.FieldValue8 = fv.Value;
                        }
                        fv = await db.field_values.SingleOrDefaultAsync(x => x.CaseId == caseId && x.FieldId == cl.Field9);
                        if (fv != null)
                        {
                            match.FieldValue9 = fv.Value;
                        }
                        fv = await db.field_values.SingleOrDefaultAsync(x => x.CaseId == caseId && x.FieldId == cl.Field10);
                        if (fv != null)
                        {
                            match.FieldValue10 = fv.Value;
                        }

                        await match.Update(db);
//                        match.Version += 1;
//                        match.UpdatedAt = DateTime.Now;
//                        //TODO! THIS part need to be redone in some form in EF Core!
//                        //db.cases.AddOrUpdate(match);
//                        db.SaveChanges();
//                        db.case_versions.Add(MapCaseVersions(match));
//                        db.SaveChanges();


                        #endregion
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("UpdateCaseFieldValue failed", ex);
            }
        }

        /// <summary>
        /// Reads check from DB with given microtingUId and checkUId
        /// </summary>
        /// <param name="microtingUId"></param>
        /// <param name="checkUId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<ReplyElement> CheckRead(int microtingUId, int checkUId)
        {
            try
            {
                using (var db = GetContext())
                {
                    var aCase = await db.cases.SingleAsync(x => x.MicrotingUid == microtingUId && x.MicrotingCheckUid == checkUId);
                    var mainCheckList = await db.check_lists.SingleAsync(x => x.Id == aCase.CheckListId);

                    ReplyElement replyElement = new ReplyElement();

                    if (aCase.CheckListId != null) replyElement.Id = (int) aCase.CheckListId;
                    replyElement.CaseType = aCase.Type;
                    replyElement.Custom = aCase.Custom;
                    if (aCase.DoneAt != null) replyElement.DoneAt = (DateTime) aCase.DoneAt;
                    if (aCase.WorkerId != null) replyElement.DoneById = (int) aCase.WorkerId;
                    replyElement.ElementList = new List<Element>();
                    //replyElement.EndDate
                    replyElement.FastNavigation = t.Bool(mainCheckList.FastNavigation);
                    replyElement.Label = mainCheckList.Label;
                    //replyElement.Language
                    replyElement.ManualSync = t.Bool(mainCheckList.ManualSync);
                    replyElement.MultiApproval = t.Bool(mainCheckList.MultiApproval);
                    if (mainCheckList.Repeated != null) replyElement.Repeated = (int) mainCheckList.Repeated;
                    //replyElement.StartDate
                    if (aCase.UnitId != null) replyElement.UnitId = (int) aCase.UnitId;
                    replyElement.MicrotingUId = (int)aCase.MicrotingCheckUid;
                    if (aCase.Site.MicrotingUid != null) replyElement.SiteMicrotingUuid = (int) aCase.Site.MicrotingUid;
                    replyElement.JasperExportEnabled = mainCheckList.JasperExportEnabled;
                    replyElement.DocxExportEnabled = mainCheckList.DocxExportEnabled;

                    foreach (check_lists checkList in aCase.CheckList.Children.OrderBy(x => x.DisplayIndex))
                    {
                        replyElement.ElementList.Add(await SubChecks(checkList.Id, aCase.Id));
                    }
                    return replyElement;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("EformCheckRead failed", ex);
            }
        }

        //TODO
        private async Task<Element> SubChecks(int parentId, int caseId)
        {
            try
            {
                using (var db = GetContext())
                {
                    var checkList = await db.check_lists.SingleAsync(x => x.Id == parentId);
                    //Element element = new Element();
                    if (checkList.Children.Count() > 0)
                    {
                        List<Element> elementList = new List<Element>();
                        foreach (check_lists subList in checkList.Children.OrderBy(x => x.DisplayIndex))
                        {
                            elementList.Add(await SubChecks(subList.Id, caseId));
                        }
                        GroupElement element = new GroupElement(checkList.Id, 
                            checkList.Label, 
                            (int)checkList.DisplayIndex, 
                            checkList.Description, 
                            t.Bool(checkList.ApprovalEnabled), 
                            t.Bool(checkList.ReviewEnabled), 
                            t.Bool(checkList.DoneButtonEnabled), 
                            t.Bool(checkList.ExtraFieldsEnabled), 
                            "", 
                            t.Bool(checkList.QuickSyncEnabled), 
                            elementList);
                        element.OriginalId = checkList.OriginalId;
                        return element;
                    }
                    else
                    {
                        List<DataItemGroup> dataItemGroupList = new List<DataItemGroup>();
                        List<DataItem> dataItemList = new List<DataItem>();
                        foreach (fields field in checkList.Fields.Where(x => x.ParentFieldId == null).OrderBy(x => x.DisplayIndex).ToList())
                        {
                            if (field.FieldType.FieldType == "FieldGroup")
                            {
                                List<DataItem> dataItemSubList = new List<DataItem>();
                                foreach (fields subField in field.Children.OrderBy(x => x.DisplayIndex))
                                {
                                    Field _field = DbFieldToField(subField);

                                    _field.FieldValues = new List<FieldValue>();
                                    foreach (field_values fieldValue in subField.FieldValues.Where(x => x.CaseId == caseId).ToList())
                                    {
                                        FieldValue answer = await ReadFieldValue(fieldValue, field, false);
                                        _field.FieldValues.Add(answer);
                                    }
                                    dataItemSubList.Add(_field);
                                }

                                CDataValue description = new CDataValue();
                                description.InderValue = field.Description;
                                FieldContainer fG = new FieldContainer(field.Id, field.Label, description, field.Color, (int)field.DisplayIndex, field.DefaultValue, dataItemSubList);
                                fG.OriginalId = field.OriginalId;
                                dataItemList.Add(fG);
                            }
                            else
                            {
                                Field _field = DbFieldToField(field);
                                _field.FieldValues = new List<FieldValue>();
                                foreach (field_values fieldValue in field.FieldValues.Where(x => x.CaseId == caseId).ToList())
                                {
                                    FieldValue answer = await ReadFieldValue(fieldValue, field, false);
                                    _field.FieldValues.Add(answer);
                                }
                                dataItemList.Add(_field);
                            }
                        }
                        DataElement dataElement = new DataElement(checkList.Id, 
                            checkList.Label, 
                            (int)checkList.DisplayIndex, 
                            checkList.Description, 
                            t.Bool(checkList.ApprovalEnabled), 
                            t.Bool(checkList.ReviewEnabled), 
                            t.Bool(checkList.DoneButtonEnabled), 
                            t.Bool(checkList.ExtraFieldsEnabled), 
                            "", 
                            t.Bool(checkList.QuickSyncEnabled), 
                            dataItemGroupList, 
                            dataItemList);
                        dataElement.OriginalId = checkList.OriginalId;
                        //return dataElement;
                        return new CheckListValue(dataElement, await CheckListValueStatusRead(caseId, checkList.Id));
                    }
                    //return element;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("EformCheckRead failed", ex);
            }
        }

        public async Task<List<field_values>> ChecksRead(int microtingUId, int checkUId)
        {
            try
            {
                using (var db = GetContext())
                {
                    var aCase = await db.cases.SingleAsync(x => x.MicrotingUid == microtingUId && x.MicrotingCheckUid == checkUId);
                    int caseId = aCase.Id;

                    List<field_values> lst = db.field_values.Where(x => x.CaseId == caseId).ToList();
                    return lst;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("EformCheckRead failed", ex);
            }
        }

        public async Task<fields> FieldReadRaw(int id)
        {
            using (var db = GetContext())
            {
                fields fieldDb = await db.fields.SingleOrDefaultAsync(x => x.Id == id);
                return fieldDb;
            }
        }

        public async Task<check_lists> CheckListRead(int id)
        {
            using (var db = GetContext())
            {
                return await db.check_lists.SingleOrDefaultAsync(x => x.Id == id);
            }
        }

        private Field DbFieldToField(fields dbField)
        {
            Field field = new Field()
            {
                Label = dbField.Label,
                Description = new CDataValue(),
                FieldType = dbField.FieldType.FieldType,
                FieldValue = dbField.DefaultValue,
                EntityGroupId = dbField.EntityGroupId,
                Color = dbField.Color,
                Id = dbField.Id
            };
            field.Description.InderValue = dbField.Description;
            
            if (field.FieldType == "SingleSelect")
            {
                field.KeyValuePairList = PairRead(dbField.KeyValuePairList);
            }

            if (field.FieldType == "MultiSelect")
            {
                field.KeyValuePairList = PairRead(dbField.KeyValuePairList);
            }
            
            return field;
        }

        public async Task<Field> FieldRead(int id)
        {

            string methodName = t.GetMethodName("SQLController");
            try
            {
                using (var db = GetContext())
                {
                    fields dbField = await db.fields.SingleOrDefaultAsync(x => x.Id == id);

                    Field field = DbFieldToField(dbField);
                    return field;
                }
            }
            catch (Exception ex)
            {
                //logger.LogException(methodName + " failed", ex, true);
                throw new Exception(methodName + " failed", ex);
            }
        }

        private async Task<FieldValue> ReadFieldValue(field_values reply, fields dbField, bool joinUploadedData)
        {
            try
            {
                using (var db = GetContext())
                {

//                    fields field = db.fields.SingleAsync(x => x.Id == reply.FieldId);
                    FieldValue fieldValue = new FieldValue();
                    fieldValue.Accuracy = reply.Accuracy;
                    fieldValue.Altitude = reply.Altitude;
                    fieldValue.Color = dbField.Color;
                    fieldValue.Date = reply.Date;
                    fieldValue.FieldId = t.Int(reply.FieldId);
                    fieldValue.FieldType = dbField.FieldType.FieldType;
                    fieldValue.DateOfDoing = t.Date(reply.DoneAt);
                    fieldValue.Description = new CDataValue();
                    fieldValue.Description.InderValue = dbField.Description;
                    fieldValue.DisplayOrder = t.Int(dbField.DisplayIndex);
                    fieldValue.Heading = reply.Heading;
                    fieldValue.Id = reply.Id;
                    fieldValue.OriginalId = reply.Field.OriginalId;
                    fieldValue.Label = dbField.Label;
                    fieldValue.Latitude = reply.Latitude;
                    fieldValue.Longitude = reply.Longitude;
                    fieldValue.Mandatory = t.Bool(dbField.Mandatory);
                    fieldValue.ReadOnly = t.Bool(dbField.ReadOnly);
                    #region answer.UploadedDataId = reply.uploaded_data_id;
                    if (reply.UploadedDataId.HasValue)
                        if (reply.UploadedDataId > 0)
                        {
                            string locations = "";
                            int uploadedDataId;
                            uploaded_data uploadedData;
                            if (joinUploadedData)
                            {
                                List<field_values> lst = db.field_values.Where(x => x.CaseId == reply.CaseId && x.FieldId == reply.FieldId).ToList();

                                foreach (field_values fV in lst)
                                {
                                    uploadedDataId = (int)fV.UploadedDataId;

                                    uploadedData = await db.uploaded_data.SingleAsync(x => x.Id == uploadedDataId);

                                    if (uploadedData.FileName != null)
                                        locations += uploadedData.FileLocation + uploadedData.FileName + Environment.NewLine;
                                    else
                                        locations += "File attached, awaiting download" + Environment.NewLine;
                                }
                                fieldValue.UploadedData = locations.TrimEnd();
                            }
                            else
                            {
                                locations = "";
                                UploadedData uploadedDataObj = new UploadedData();
                                uploadedData = reply.UploadedData;
                                uploadedDataObj.Checksum = uploadedData.Checksum;
                                uploadedDataObj.Extension = uploadedData.Extension;
                                uploadedDataObj.CurrentFile = uploadedData.CurrentFile;
                                uploadedDataObj.UploaderId = uploadedData.UploaderId;
                                uploadedDataObj.UploaderType = uploadedData.UploaderType;
                                uploadedDataObj.FileLocation = uploadedData.FileLocation;
                                uploadedDataObj.FileName = uploadedData.FileName;
                                uploadedDataObj.Id = uploadedData.Id;
                                fieldValue.UploadedDataObj = uploadedDataObj;
                                fieldValue.UploadedData = "";
                            }

                        }
                    #endregion
                    fieldValue.Value = reply.Value;
                    #region answer.ValueReadable = reply.value 'ish' //and if needed: answer.KeyValuePairList = ReadPairs(...);
                    fieldValue.ValueReadable = reply.Value;

                    if (fieldValue.FieldType == Constants.Constants.FieldTypes.EntitySearch || fieldValue.FieldType == Constants.Constants.FieldTypes.EntitySelect)
                    {
                        try
                        {
                            if (reply.Value != "" || reply.Value != null)
                            {
								int Id = int.Parse(reply.Value);
                                entity_items match = await db.entity_items.SingleOrDefaultAsync(x => x.Id == Id);

                                if (match != null)
                                {
                                    fieldValue.ValueReadable = match.Name;
                                    fieldValue.Value = match.Id.ToString();
                                    fieldValue.MicrotingUuid = match.MicrotingUid;
                                }

                            }
                        }
                        catch { }
                    }

                    if (fieldValue.FieldType == Constants.Constants.FieldTypes.SingleSelect)
                    {
                        string key = fieldValue.Value;
                        string fullKey = t.Locate(dbField.KeyValuePairList, "<" + key + ">", "</" + key + ">");
                        fieldValue.ValueReadable = t.Locate(fullKey, "<key>", "</key>");

                        fieldValue.KeyValuePairList = PairRead(dbField.KeyValuePairList);
                    }

                    if (fieldValue.FieldType == Constants.Constants.FieldTypes.MultiSelect)
                    {
                        fieldValue.ValueReadable = "";

                        string keys = fieldValue.Value;
                        List<string> keyLst = keys.Split('|').ToList();

                        foreach (string key in keyLst)
                        {
                            string fullKey = t.Locate(dbField.KeyValuePairList, "<" + key + ">", "</" + key + ">");
                            if (fieldValue.ValueReadable != "")
                                fieldValue.ValueReadable += '|';
                            fieldValue.ValueReadable += t.Locate(fullKey, "<key>", "</key>");
                        }

                        fieldValue.KeyValuePairList = PairRead(dbField.KeyValuePairList);
                    }

                    if (fieldValue.FieldType == Constants.Constants.FieldTypes.Number ||
                        fieldValue.FieldType == Constants.Constants.FieldTypes.NumberStepper)
                    {
                        if (reply.Value != null)
                        {
                            fieldValue.ValueReadable = reply.Value.Replace(",", ".");
                            fieldValue.Value = reply.Value.Replace(",", ".");
                        }
                        else
                        {
                            fieldValue.ValueReadable = "";
                            fieldValue.Value = "";
                        }
                    }
                    #endregion

                    return fieldValue;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("FieldValueRead failed", ex);
            }
        }

        // Rename method to something more intuitive!
        public async Task<FieldValue> FieldValueRead(field_values reply, bool joinUploadedData)
        {
            
            
            try
            {
                using (var db = GetContext())
                {

                    fields field = await db.fields.SingleAsync(x => x.Id == reply.FieldId);

                    return await ReadFieldValue(reply, field, joinUploadedData);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("FieldValueRead failed", ex);
            }
        }

        public async Task<List<FieldValue>> FieldValueReadList(int fieldId, int instancesBackInTime)
        {
            try
            {
                using (var db = GetContext())
                {
                    List<field_values> matches = db.field_values.Where(x => x.FieldId == fieldId).OrderByDescending(z => z.CreatedAt).ToList();

                    if (matches.Count() > instancesBackInTime)
                        matches = matches.GetRange(0, instancesBackInTime);

                    List<FieldValue> rtnLst = new List<FieldValue>();

                    foreach (var item in matches)
                        rtnLst.Add(await FieldValueRead(item, true));

                    return rtnLst;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("FieldValueReadList failed", ex);
            }
        }

        public async Task<List<FieldValue>> FieldValueReadList(int fieldId, List<int> caseIds)
        {
            try
            {
                using (var db = GetContext())
                {
                    List<field_values> matches = db.field_values.Where(x => x.FieldId == fieldId && caseIds.Contains(x.Id)).ToList();
                    
                    List<FieldValue> rtnLst = new List<FieldValue>();

                    foreach (var item in matches)
                        rtnLst.Add(await FieldValueRead(item, true));

                    return rtnLst;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("FieldValueReadList failed", ex);
            }
        }

        public async Task<List<FieldValue>> FieldValueReadList(List<int> caseIds)
        {
            try
            {
                using (var db = GetContext())
                {
                    List<field_values> matches = db.field_values.Where(x => caseIds.Contains((int)x.CaseId)).ToList();
                    List<FieldValue> rtnLst = new List<FieldValue>();

                    foreach (var item in matches)
                        rtnLst.Add(await FieldValueRead(item, false));

                    return rtnLst;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("FieldValueReadList failed", ex);
            }
        }
        
#pragma warning disable 1998
        public async Task<List<CheckListValue>> CheckListValueReadList(List<int> caseIds)
        {
            try
            {
                using (var db = GetContext())
                {
                    List<check_list_values> matches = db.check_list_values.Where(x => caseIds.Contains((int)x.CaseId)).ToList();
                    List<CheckListValue> rtnLst = new List<CheckListValue>();

                    foreach (var item in matches)
                    {
                        CheckListValue checkListValue = new CheckListValue();
                        checkListValue.Id = item.Id;
                        checkListValue.Label = item.Status;
                        rtnLst.Add(checkListValue);
                    }
                        

                    return rtnLst;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("FieldValueReadList failed", ex);
            }
        }
#pragma warning restore 1998


        public async Task FieldValueUpdate(int caseId, int fieldValueId, string value)
        {
            try
            {
                using (var db = GetContext())
                {
                    field_values fieldMatch = await db.field_values.SingleAsync(x => x.Id == fieldValueId);

                    fieldMatch.Value = value;
                    await fieldMatch.Update(db);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("FieldValueUpdate failed", ex);
            }
        }

        public async Task<List<List<KeyValuePair>>> FieldValueReadAllValues(int fieldId, List<int> caseIds,
            string customPathForUploadedData)
        {
            return await FieldValueReadAllValues(fieldId, caseIds, customPathForUploadedData, ".", "");
        }

        public async Task<List<List<KeyValuePair>>> FieldValueReadAllValues(int fieldId, List<int> caseIds, string customPathForUploadedData, string decimalSeparator, string thousandSeparator)
        {
            try
            {
                using (var db = GetContext())
                {
                    fields matchField = await db.fields.SingleAsync(x => x.Id == fieldId);

                    List<field_values> matches = db.field_values.Where(x => x.FieldId == fieldId && caseIds.Contains((int)x.CaseId)).ToList();

                    List<List<KeyValuePair>> rtrnLst = new List<List<KeyValuePair>>();
                    List<KeyValuePair> replyLst1 = new List<KeyValuePair>();
                    rtrnLst.Add(replyLst1);

                    switch (matchField.FieldType.FieldType)
                    {
                        #region special dataItem
                        case Constants.Constants.FieldTypes.CheckBox:
                            foreach (field_values item in matches)
                            {
                                if (item.Value == "checked")
                                    replyLst1.Add(new KeyValuePair(item.CaseId.ToString(), "1", false, ""));
                                else
                                    replyLst1.Add(new KeyValuePair(item.CaseId.ToString(), "0", false, ""));
                            }
                            break;
                        case Constants.Constants.FieldTypes.Signature:
                        case Constants.Constants.FieldTypes.Picture:
                            int lastCaseId = -1;
                            int lastIndex = -1;
                            foreach (field_values item in matches)
                            {
                                if (item.Value != null)
                                {
                                    if (lastCaseId == (int)item.CaseId)
                                    {

                                        foreach (KeyValuePair kvp in replyLst1)
                                        {
                                            if (kvp.Key == item.CaseId.ToString())
                                            {
                                                if (customPathForUploadedData != null)
                                                    if (kvp.Value.Contains("jpg") || kvp.Value.Contains("jpeg") || kvp.Value.Contains("png"))
                                                        kvp.Value = kvp.Value + "|" + customPathForUploadedData + item.UploadedData.FileName;
                                                    else
                                                        kvp.Value = customPathForUploadedData + item.UploadedData.FileName;
                                                else
                                                    if (kvp.Value.Contains("jpg") || kvp.Value.Contains("jpeg") || kvp.Value.Contains("png"))
                                                    kvp.Value = kvp.Value + "|" + item.UploadedData.FileLocation + item.UploadedData.FileName;
                                                else
                                                    kvp.Value = item.UploadedData.FileLocation + item.UploadedData.FileName;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        lastIndex++;
                                        if (item.UploadedDataId != null)
                                        {
                                            if (customPathForUploadedData != null)

                                                replyLst1.Add(new KeyValuePair(item.CaseId.ToString(), customPathForUploadedData + item.UploadedData.FileName, false, ""));
                                            else
                                                replyLst1.Add(new KeyValuePair(item.CaseId.ToString(), item.UploadedData.FileLocation + item.UploadedData.FileName, false, ""));
                                        }
                                        else
                                        {
                                            replyLst1.Add(new KeyValuePair(item.CaseId.ToString(), "", false, ""));
                                        }
                                    }
                                }
                                else
                                {
                                    lastIndex++;
                                    replyLst1.Add(new KeyValuePair(item.CaseId.ToString(), "", false, ""));
                                }
                                lastCaseId = (int)item.CaseId;
                            }
                            break;

                        case Constants.Constants.FieldTypes.SingleSelect:
                            {
                                var kVP = PairRead(matchField.KeyValuePairList);

                                foreach (field_values item in matches)
                                    replyLst1.Add(new KeyValuePair(item.CaseId.ToString(), PairMatch(kVP, item.Value), false, ""));
                            }
                            break;

                        case Constants.Constants.FieldTypes.MultiSelect:
                            {
                                var kVP = PairRead(matchField.KeyValuePairList);
                                rtrnLst = new List<List<KeyValuePair>>();
                                List<KeyValuePair> replyLst = null;
                                int index = 0;
                                string valueExt = "";

                                foreach (var key in kVP)
                                {
                                    replyLst = new List<KeyValuePair>();
                                    index++;

                                    foreach (field_values item in matches)
                                    {
                                        valueExt = "|" + item.Value + "|";
                                        if (valueExt.Contains("|" + index.ToString() + "|"))
                                            replyLst.Add(new KeyValuePair(item.CaseId.ToString(), "1", false, ""));
                                        else
                                            replyLst.Add(new KeyValuePair(item.CaseId.ToString(), "0", false, ""));
                                    }

                                    rtrnLst.Add(replyLst);
                                }

                            }
                            break;

                        case Constants.Constants.FieldTypes.EntitySelect:
                        case Constants.Constants.FieldTypes.EntitySearch:
                            {
                                foreach (field_values item in matches)
                                {
                                    try
                                    {
                                        if (item.Value != "" || item.Value != null)
                                        {
                                            entity_items match = await db.entity_items.SingleOrDefaultAsync(x => x.Id.ToString() == item.Value);

                                            if (match != null)
                                            {
                                                replyLst1.Add(new KeyValuePair(item.CaseId.ToString(), match.Name, false, ""));
                                            }
                                            else
                                            {
                                                replyLst1.Add(new KeyValuePair(item.CaseId.ToString(), "", false, ""));
                                            }

                                        }
                                    }
                                    catch
                                    {
                                        replyLst1.Add(new KeyValuePair(item.CaseId.ToString(), "", false, ""));
                                    }
                                }
                            }
                            break;
                        case Constants.Constants.FieldTypes.Number:
                            foreach (field_values item in matches)
                            {
                                //string value = item.value.Replace(".", decimalSeparator);
                                if (!string.IsNullOrEmpty(thousandSeparator))
                                {
                                    switch (thousandSeparator)
                                    {
                                        case ".":
                                        {
                                            replyLst1.Add(new KeyValuePair(item.CaseId.ToString(), String.Format("{0:#.##0.##}", item.Value), false, ""));
                                        }   
                                        break;
                                        case ",":
                                        {
                                            replyLst1.Add(new KeyValuePair(item.CaseId.ToString(), String.Format("{0:#,##0.##}", item.Value), false, ""));
                                        }
                                        break;

                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(decimalSeparator))
                                    {
                                        string value = "";
                                        if (item.Value != null)
                                        {
                                            value = item.Value.Replace(".", decimalSeparator).Replace(",", decimalSeparator);    
                                        }
                                        replyLst1.Add(new KeyValuePair(item.CaseId.ToString(), value, false, ""));   
                                    }
                                    else
                                    {
                                        string value = "";
                                        if (item.Value != null)
                                        {
                                            value = item.Value.Replace(".", decimalSeparator).Replace(",", decimalSeparator);    
                                        }
                                        replyLst1.Add(new KeyValuePair(item.CaseId.ToString(), value, false, ""));
                                    }                                    
                                }
                            }
                            break;
                        #endregion

                        default:
                            foreach (field_values item in matches)
                            {
                                replyLst1.Add(new KeyValuePair(item.CaseId.ToString(), item.Value, false, ""));
                            }
                            break;
                    }

                    return rtrnLst;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CaseReadFull failed", ex);
            }
        }

        public async Task<string> CheckListValueStatusRead(int caseId, int checkListId)
        {
            try
            {
                using (var db = GetContext())
                {
                    check_list_values clv = await db.check_list_values.SingleAsync(x => x.CaseId == caseId && x.CheckListId == checkListId);
                    return clv.Status;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CheckListValueStatusRead failed", ex);
            }
        }

        public async Task CheckListValueStatusUpdate(int caseId, int checkListId, string value)
        {
            try
            {
                using (var db = GetContext())
                {
                    check_list_values match = await db.check_list_values.SingleAsync(x => x.CaseId == caseId && x.CheckListId == checkListId);

                    match.Status = value;
                    await match.Update(db);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("FieldValueUpdate failed", ex);
            }
        }
        #endregion

        #region notification
        
        /// <summary>
        /// Creates Notification in DB with given values notificationUId, microtingUId and activity
        /// </summary>
        /// <param name="notificationUId"></param>
        /// <param name="microtingUId"></param>
        /// <param name="activity"></param>
        /// <returns></returns>
        public async Task<notifications> NotificationCreate(string notificationUId, int microtingUId, string activity)
        {
            string methodName = t.GetMethodName("SQLController");

            using (var db = GetContext())
            {
                if (!db.notifications.Any(x => x.NotificationUid == notificationUId && x.MicrotingUid == microtingUId))
                {
                    await log.LogStandard(methodName, "SAVING notificationUId : " + notificationUId + " microtingUId : " + microtingUId + " action : " + activity);

                    notifications aNote = new notifications();

                    aNote.NotificationUid = notificationUId;
                    aNote.MicrotingUid = microtingUId;
                    aNote.Activity = activity;
                    await aNote.Create(db);
                    return aNote;
                }
                else
                {
                    notifications aNote = await db.notifications.SingleOrDefaultAsync(x => x.NotificationUid == notificationUId && x.MicrotingUid == microtingUId);
                    return aNote;
                }

                //TODO else log warning
            }
        }

        /// <summary>
        /// Returns a Note Data transfer object from DB
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Note_Dto> NotificationReadFirst()
        {
            try
            {
                using (var db = GetContext())
                {
                    notifications aNoti = await db.notifications.FirstOrDefaultAsync(x => x.WorkflowState == Constants.Constants.WorkflowStates.Created);

                    if (aNoti != null)
                    {
                        Note_Dto aNote = new Note_Dto(aNoti.NotificationUid, aNoti.MicrotingUid, aNoti.Activity);
                        return aNote;
                    }
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(t.GetMethodName("SQLController") + " failed", ex);
            }
        }

        /// <summary>
        /// Updates a Notification in DB with new values of notificationUId, microtingUId, workflowstate, exception and stack trace
        /// </summary>
        /// <param name="notificationUId"></param>
        /// <param name="microtingUId"></param>
        /// <param name="workflowState"></param>
        /// <param name="exception"></param>
        /// <param name="stacktrace"></param>
        /// <exception cref="Exception"></exception>
        public async Task NotificationUpdate(string notificationUId, int microtingUId, string workflowState, string exception, string stacktrace)
        {
            try
            {
                using (var db = GetContext())
                {
                    notifications aNoti = await db.notifications.SingleAsync(x => x.NotificationUid == notificationUId && x.MicrotingUid == microtingUId);
                    aNoti.WorkflowState = workflowState;
//                    aNoti.UpdatedAt = DateTime.Now;
                    aNoti.Exception = exception;
                    aNoti.Stacktrace = stacktrace;
                    await aNoti.Update(db);

//                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(t.GetMethodName("SQLController") + " failed", ex);
            }
        }
        #endregion

        #region file
        
        //TODO
        public async Task<UploadedData> FileRead()
        {
            try
            {
                using (var db = GetContext())
                {
                    uploaded_data dU = await db.uploaded_data.FirstOrDefaultAsync(x => x.WorkflowState == Constants.Constants.WorkflowStates.PreCreated);

                    if (dU != null)
                    {
                        UploadedData ud = new UploadedData();
                        ud.Checksum = dU.Checksum;
                        ud.Extension = dU.Extension;
                        ud.CurrentFile = dU.CurrentFile;
                        ud.UploaderId = dU.UploaderId;
                        ud.UploaderType = dU.UploaderType;
                        ud.FileLocation = dU.FileLocation;
                        ud.FileName = dU.FileName;
                        ud.Id = dU.Id;
                        return ud;
                    }
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("FileRead failed", ex);
            }
        }

        //TODO
        public async Task<Case_Dto> FileCaseFindMUId(string urlString)
        {
            try
            {
                using (var db = GetContext())
                {
                    try
                    {
                        uploaded_data dU = db.uploaded_data.Where(x => x.FileLocation == urlString).First();
                        field_values fV = await db.field_values.SingleAsync(x => x.UploadedDataId == dU.Id);
                        cases aCase = await db.cases.SingleAsync(x => x.Id == fV.CaseId);

                        return await CaseReadByCaseId(aCase.Id);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
//                await log.LogCritical(t.GetMethodName("Core"), ex.Message);
                throw new Exception("FileRead failed", ex);
            }
        }

        //TODO
        public async Task FileProcessed(string urlString, string checkSum, string fileLocation, string fileName, int Id)
        {
            try
            {
                using (var db = GetContext())
                {
                    uploaded_data uD = await db.uploaded_data.SingleAsync(x => x.Id == Id);

                    uD.Checksum = checkSum;
                    uD.FileLocation = fileLocation;
                    uD.FileName = fileName;
                    uD.Local = 1;
                    uD.WorkflowState = Constants.Constants.WorkflowStates.Created;
                    await uD.Update(db);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("FileProcessed failed", ex);
            }
        }

        /// <summary>
        /// Returns uploaded data object from DB with give Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<uploaded_data> GetUploadedData(int Id)
        {
            try
            {
                using (var db = GetContext())
                {
                    return await db.uploaded_data.SingleOrDefaultAsync(x => x.Id == Id);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Get uploaded data object failed", ex);
            }
        }

        //TODO
        public async Task<bool> UpdateUploadedData(uploaded_data uploadedData)
        {
            try
            {
                using (var db = GetContext())
                {
                    uploaded_data uD = await db.uploaded_data.SingleAsync(x => x.Id == uploadedData.Id);
                    uD.TranscriptionId = uploadedData.TranscriptionId;
                    await uD.Update(db);
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("UpdateUploadedData failed", ex);
            }
        }

        /// <summary>
        /// Returns field_values object from DB with given transcription Id
        /// </summary>
        /// <param name="transcriptionId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<field_values> GetFieldValueByTranscriptionId(int transcriptionId)
        {
            try
            {
                using (var db = GetContext())
                {
                    uploaded_data ud = await GetUploaded_DataByTranscriptionId(transcriptionId);
                    if (ud != null)
                    {
                        return await db.field_values.SingleOrDefaultAsync(x => x.UploadedDataId == ud.Id);
                    } else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Get uploaded data object failed", ex);
            }
        }

        //TODO
        public async Task<uploaded_data> GetUploaded_DataByTranscriptionId(int transcriptionId)
        {

            try
            {
                using (var db = GetContext())
                {
                    return await db.uploaded_data.SingleOrDefaultAsync(x => x.TranscriptionId == transcriptionId);                    
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Get uploaded data object failed", ex);
            }
        }

        /// <summary>
        /// Deletes file from DB with given Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> DeleteFile(int Id)
        {
            try
            {
                using (var db = GetContext())
                {
                    uploaded_data uD = await db.uploaded_data.SingleAsync(x => x.Id == Id);

                    await uD.Delete(db);

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("FileProcessed failed", ex);
            }
        }
        #endregion
        #endregion

        #region public (post)case

        /// <summary>
        /// Return a case data transfer object from DB with given microtingUId and checkUId
        /// </summary>
        /// <param name="microtingUId"></param>
        /// <param name="checkUId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Case_Dto> CaseLookup(int microtingUId, int checkUId)
        {
            try
            {
                using (var db = GetContext())
                {
                    cases aCase = await db.cases.SingleAsync(x => x.MicrotingUid == microtingUId && x.MicrotingCheckUid == checkUId);
                    return await CaseReadByCaseId(aCase.Id);
                }
            } catch  (Exception ex)
            {
                throw new Exception("CaseReadByMuuId failed", ex);
            }
        }
        
        /// <summary>
        /// Finds a Case_Dto based on microtingUId
        /// </summary>
        /// <param name="microtingUId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Case_Dto> CaseReadByMUId(int microtingUId)
        {
            try
            {
                using (var db = GetContext())
                {
                    try
                    {
                        cases aCase = await db.cases.SingleAsync(x => x.MicrotingUid == microtingUId);
                        return await CaseReadByCaseId(aCase.Id);
                    }
                    catch { }

                    try
                    {
                        check_list_sites cls = await db.check_list_sites.SingleAsync(x => x.MicrotingUid == microtingUId);
                        check_lists cL = await db.check_lists.SingleAsync(x => x.Id == cls.CheckListId);

                        #region string stat = aCase.workflow_state ...
                        string stat = "";
                        if (cls.WorkflowState == Constants.Constants.WorkflowStates.Created)
                            stat = Constants.Constants.WorkflowStates.Created;

                        if (cls.WorkflowState == Constants.Constants.WorkflowStates.Removed)
                            stat = "Deleted";
                        #endregion

                        int remoteSiteId = (int)db.sites.SingleAsync(x => x.Id == (int)cls.SiteId).Result.MicrotingUid;
                        Case_Dto rtrnCase = new Case_Dto(null, stat, remoteSiteId, cL.CaseType, "ReversedCase", cls.MicrotingUid, cls.LastCheckId, null, cL.Id, null);
                        return rtrnCase;
                    }
                    catch(Exception ex1)
                    {
                        throw new Exception("CaseReadByMuuId failed", ex1);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CaseReadByMuuId failed", ex);
            }
        }

        /// <summary>
        /// Reads a Case from DB with given case id
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Case_Dto> CaseReadByCaseId(int caseId)
        {
            try
            {
                using (var db = GetContext())
                {
                    cases aCase = await db.cases.SingleAsync(x => x.Id == caseId);
                    check_lists cL = await db.check_lists.SingleAsync(x => x.Id == aCase.CheckListId);

                    #region string stat = aCase.workflow_state ...
                    string stat = "";
                    if (aCase.WorkflowState == Constants.Constants.WorkflowStates.Created && aCase.Status != 77)
                        stat = "Created";

                    if (aCase.WorkflowState == Constants.Constants.WorkflowStates.Created && aCase.Status == 77)
                        stat = "Retrived";

                    if (aCase.WorkflowState == Constants.Constants.WorkflowStates.Retracted)
                        stat = "Completed";

                    if (aCase.WorkflowState == Constants.Constants.WorkflowStates.Removed)
                        stat = "Deleted";
                    #endregion

                    int remoteSiteId = (int)db.sites.SingleAsync(x => x.Id == (int)aCase.SiteId).Result.MicrotingUid;
                    Case_Dto cDto = new Case_Dto(aCase.Id, stat, remoteSiteId, cL.CaseType, aCase.CaseUid, aCase.MicrotingUid, aCase.MicrotingCheckUid, aCase.Custom, cL.Id, aCase.WorkflowState);
                    return cDto;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CaseReadByCaseId failed", ex);
            }
        }

        /// <summary>
        /// Return a list of Case data transfer objects from db with given caseUId
        /// </summary>
        /// <param name="caseUId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<Case_Dto>> CaseReadByCaseUId(string caseUId)
        {
            try
            {
                if (caseUId == "")
                    throw new Exception("CaseReadByCaseUId failed. Due invalid input:''. This would return incorrect data");

                if (caseUId == "ReversedCase")
                    throw new Exception("CaseReadByCaseUId failed. Due invalid input:'ReversedCase'. This would return incorrect data");

                using (var db = GetContext())
                {
                    List<cases> matches = db.cases.Where(x => x.CaseUid == caseUId).ToList();
                    List<Case_Dto> lstDto = new List<Case_Dto>();

                    foreach (cases aCase in matches)
                        lstDto.Add(await CaseReadByCaseId(aCase.Id));

                    return lstDto;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CaseReadByCaseUId failed", ex);
            }
        }
        
        /// <summary>
        /// Returns a cases object from DB with given microtingUId and checkUId
        /// </summary>
        /// <param name="microtingUId"></param>
        /// <param name="checkUId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<cases> CaseReadFull(int microtingUId, int checkUId)
        {
            try
            {
                using (var db = GetContext())
                {
                    cases match = await db.cases.SingleOrDefaultAsync(x => x.MicrotingUid == microtingUId && x.MicrotingCheckUid == checkUId);
                    match.SiteId = db.sites.SingleOrDefaultAsync(x => x.Id == match.SiteId).Result.MicrotingUid;

                    if (match.UnitId != null)
                        match.UnitId = db.units.SingleOrDefaultAsync(x => x.Id == match.UnitId).Result.MicrotingUid;

                    if (match.WorkerId != null)
                        match.WorkerId = db.workers.SingleOrDefaultAsync(x => x.Id == match.WorkerId).Result.MicrotingUid;
                    return match;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CaseReadFull failed", ex);
            }
        }

        /// <summary>
        /// Returns an ID of a specific Case from DB with given templateId and workflowstate
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="workflowState"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int?> CaseReadFirstId(int? templateId, string workflowState)
        {
            string methodName = t.GetMethodName("SQLController");
            await log.LogStandard(methodName, "called");
            await log.LogVariable(methodName, nameof(templateId), templateId);
            await log.LogVariable(methodName, nameof(workflowState), workflowState);
            try
            {
                using (var db = GetContext())
                {
                    //cases dbCase = null;
                    IQueryable<cases> sub_query = db.cases.Where(x => x.CheckListId == templateId && x.Status == 100);
                    switch (workflowState)
                    {
                        case Constants.Constants.WorkflowStates.NotRetracted:
                            sub_query = sub_query.Where(x => x.WorkflowState != Constants.Constants.WorkflowStates.Retracted);
                            break;
                        case Constants.Constants.WorkflowStates.NotRemoved:
                            sub_query = sub_query.Where(x => x.WorkflowState != Constants.Constants.WorkflowStates.Removed);
                            break;
                        case Constants.Constants.WorkflowStates.Created:
                            sub_query = sub_query.Where(x => x.WorkflowState == Constants.Constants.WorkflowStates.Created);
                            break;
                        case Constants.Constants.WorkflowStates.Retracted:
                            sub_query = sub_query.Where(x => x.WorkflowState == Constants.Constants.WorkflowStates.Retracted);
                            break;
                        case Constants.Constants.WorkflowStates.Removed:
                            sub_query = sub_query.Where(x => x.WorkflowState == Constants.Constants.WorkflowStates.Removed);
                            break;
                        default:
                            break;
                    }

                    try
                    {
                        return sub_query.First().Id;
                    } catch (Exception ex)
                    {
                        throw new Exception("CaseReadFirstId failed", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CaseReadFull failed", ex);
            }
        }

        //TODO
        public async Task<CaseList> CaseReadAll(int? templatId, DateTime? start, DateTime? end, string workflowState,
            string searchKey, bool descendingSort, string sortParameter, int offSet, int pageSize)
        {
                        
            try
            {
                using (var db = GetContext())
                {
                    if (start == null)
                        start = DateTime.MinValue;
                    if (end == null)
                        end = DateTime.MaxValue;

                    //db.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);

                    List<cases> matches = null;
                    IQueryable<cases> sub_query = db.cases.Where(x => x.DoneAt > start && x.DoneAt < end);
                    switch (workflowState)
                    {
                        case Constants.Constants.WorkflowStates.NotRetracted:
                            sub_query = sub_query.Where(x => x.WorkflowState != Constants.Constants.WorkflowStates.Retracted);
                            break;
                        case Constants.Constants.WorkflowStates.NotRemoved:
                            sub_query = sub_query.Where(x => x.WorkflowState != Constants.Constants.WorkflowStates.Removed);
                            break;
                        case Constants.Constants.WorkflowStates.Created:
                            sub_query = sub_query.Where(x => x.WorkflowState == Constants.Constants.WorkflowStates.Created);
                            break;
                        case Constants.Constants.WorkflowStates.Retracted:
                            sub_query = sub_query.Where(x => x.WorkflowState == Constants.Constants.WorkflowStates.Retracted);
                            break;
                        case Constants.Constants.WorkflowStates.Removed:
                            sub_query = sub_query.Where(x => x.WorkflowState == Constants.Constants.WorkflowStates.Removed);
                            break;
                        default:
                            break;
                    }


                    if (templatId != null)
                    {
                        sub_query = sub_query.Where(x => x.CheckListId == templatId);
                    }
                    
                    if (!string.IsNullOrEmpty(searchKey))
                    {
                        if (searchKey.Contains("!"))
                        {
                            searchKey = searchKey.ToLower().Replace("!", "");
                            IQueryable<cases> excludeQuery = db.cases.Where(x => x.DoneAt > start && x.DoneAt < end);
                            excludeQuery = excludeQuery.Where(x => x.FieldValue1.ToLower().Contains(searchKey) || 
                                                             x.FieldValue2.ToLower().Contains(searchKey) || 
                                                             x.FieldValue3.ToLower().Contains(searchKey) || 
                                                             x.FieldValue4.ToLower().Contains(searchKey) || 
                                                             x.FieldValue5.ToLower().Contains(searchKey) || 
                                                             x.FieldValue6.ToLower().Contains(searchKey) || 
                                                             x.FieldValue7.ToLower().Contains(searchKey) || 
                                                             x.FieldValue8.ToLower().Contains(searchKey) || 
                                                             x.FieldValue9.ToLower().Contains(searchKey) || 
                                                             x.FieldValue10.ToLower().Contains(searchKey) || 
                                                             x.Id.ToString().Contains(searchKey) || 
                                                             x.Site.Name.ToLower().Contains(searchKey) || 
                                                             x.Worker.FirstName.ToLower().Contains(searchKey) || 
                                                             x.Worker.LastName.ToLower().Contains(searchKey) || 
                                                             x.DoneAt.ToString().Contains(searchKey));
                            
                            sub_query = sub_query.Except(excludeQuery.ToList());
                        }
                        else
                        {
                            searchKey = searchKey.ToLower();
                            sub_query = sub_query.Where(x => x.FieldValue1.ToLower().Contains(searchKey) || 
                                                             x.FieldValue2.ToLower().Contains(searchKey) || 
                                                             x.FieldValue3.ToLower().Contains(searchKey) || 
                                                             x.FieldValue4.ToLower().Contains(searchKey) || 
                                                             x.FieldValue5.ToLower().Contains(searchKey) || 
                                                             x.FieldValue6.ToLower().Contains(searchKey) || 
                                                             x.FieldValue7.ToLower().Contains(searchKey) || 
                                                             x.FieldValue8.ToLower().Contains(searchKey) || 
                                                             x.FieldValue9.ToLower().Contains(searchKey) || 
                                                             x.FieldValue10.ToLower().Contains(searchKey) || 
                                                             x.Id.ToString().Contains(searchKey) || 
                                                             x.Site.Name.ToLower().Contains(searchKey) || 
                                                             x.Worker.FirstName.ToLower().Contains(searchKey) || 
                                                             x.Worker.LastName.ToLower().Contains(searchKey) ||
                                                             x.DoneAt.ToString().Contains(searchKey));    
                        }
                    }

                    switch (sortParameter)
                    {
                        case Constants.Constants.CaseSortParameters.CreatedAt:
                            if (descendingSort)
                                sub_query = sub_query.OrderByDescending(x => x.Id);
                            else
                                sub_query = sub_query.OrderBy(x => x.Id);
                            break;
                        case Constants.Constants.CaseSortParameters.DoneAt:
                            if (descendingSort)
                                sub_query = sub_query.OrderByDescending(x => x.DoneAt);
                            else
                                sub_query = sub_query.OrderBy(x => x.DoneAt);
                            break;
                        case Constants.Constants.CaseSortParameters.WorkerName:
                            if (descendingSort)
                                sub_query = sub_query.OrderByDescending(x => x.Worker.FirstName);
                            else
                                sub_query = sub_query.OrderBy(x => x.Worker.FirstName);
                            break;
                        case Constants.Constants.CaseSortParameters.SiteName:
                            if (descendingSort)
                                sub_query = sub_query.OrderByDescending(x => x.Site.Name);
                            else
                                sub_query = sub_query.OrderBy(x => x.Site.Name);
                            break;
                        case Constants.Constants.CaseSortParameters.UnitId:
                            if (descendingSort)
                                sub_query = sub_query.OrderByDescending(x => x.UnitId);
                            else
                                sub_query = sub_query.OrderBy(x => x.UnitId);
                            break;
                        case Constants.Constants.CaseSortParameters.Status:
                            if (descendingSort)
                                sub_query = sub_query.OrderByDescending(x => x.Status);
                            else
                                sub_query = sub_query.OrderBy(x => x.Status);
                            break;
                        case Constants.Constants.CaseSortParameters.Field1:
                            if (descendingSort)
                                sub_query = sub_query.OrderByDescending(x => x.FieldValue1);
                            else
                                sub_query = sub_query.OrderBy(x => x.FieldValue1);
                            break;
                        case Constants.Constants.CaseSortParameters.Field2:
                            if (descendingSort)
                                sub_query = sub_query.OrderByDescending(x => x.FieldValue2);
                            else
                                sub_query = sub_query.OrderBy(x => x.FieldValue2);
                            break;
                        case Constants.Constants.CaseSortParameters.Field3:
                            if (descendingSort)
                                sub_query = sub_query.OrderByDescending(x => x.FieldValue3);
                            else
                                sub_query = sub_query.OrderBy(x => x.FieldValue3);
                            break;
                        case Constants.Constants.CaseSortParameters.Field4:
                            if (descendingSort)
                                sub_query = sub_query.OrderByDescending(x => x.FieldValue4);
                            else
                                sub_query = sub_query.OrderBy(x => x.FieldValue4);
                            break;
                        case Constants.Constants.CaseSortParameters.Field5:
                            if (descendingSort)
                                sub_query = sub_query.OrderByDescending(x => x.FieldValue5);
                            else
                                sub_query = sub_query.OrderBy(x => x.FieldValue5);
                            break;
                        case Constants.Constants.CaseSortParameters.Field6:
                            if (descendingSort)
                                sub_query = sub_query.OrderByDescending(x => x.FieldValue6);
                            else
                                sub_query = sub_query.OrderBy(x => x.FieldValue6);
                            break;
                        case Constants.Constants.CaseSortParameters.Field7:
                            if (descendingSort)
                                sub_query = sub_query.OrderByDescending(x => x.FieldValue7);
                            else
                                sub_query = sub_query.OrderBy(x => x.FieldValue7);
                            break;
                        case Constants.Constants.CaseSortParameters.Field8:
                            if (descendingSort)
                                sub_query = sub_query.OrderByDescending(x => x.FieldValue8);
                            else
                                sub_query = sub_query.OrderBy(x => x.FieldValue8);
                            break;
                        case Constants.Constants.CaseSortParameters.Field9:
                            if (descendingSort)
                                sub_query = sub_query.OrderByDescending(x => x.FieldValue9);
                            else
                                sub_query = sub_query.OrderBy(x => x.FieldValue9);
                            break;
                        case Constants.Constants.CaseSortParameters.Field10:
                            if (descendingSort)
                                sub_query = sub_query.OrderByDescending(x => x.FieldValue10);
                            else
                                sub_query = sub_query.OrderBy(x => x.FieldValue10);
                            break;
                        default:
                            if (descendingSort)
                                sub_query = sub_query.OrderByDescending(x => x.Id);
                            else
                                sub_query = sub_query.OrderBy(x => x.Id);
                            break;
                    }

//                    string bla = sub_query.ToSql(db);
//                    await log.LogStandard("SQLController", $"Query is {bla}");
                    matches = await sub_query.ToListAsync();
                    
                    List<Case> rtrnLst = new List<Case>();
                    int numOfElements = 0;
                    numOfElements = matches.Count();
                    List<cases> dbCases = null;

                    if (numOfElements < pageSize)
                    {
                        dbCases = matches.ToList();
                    }
                    else
                    {
                        offSet = offSet * pageSize;
                        dbCases = matches.Skip(offSet).Take(pageSize).ToList();
                    }

                    #region cases -> Case
                    foreach (var dbCase in dbCases)
                    {
                        Case nCase = new Case();
                        nCase.CaseType = dbCase.Type;
                        nCase.CaseUId = dbCase.CaseUid;
                        nCase.CheckUIid = dbCase.MicrotingCheckUid;
                        nCase.CreatedAt = dbCase.CreatedAt;
                        nCase.Custom = dbCase.Custom;
                        nCase.DoneAt = dbCase.DoneAt;
                        nCase.Id = dbCase.Id;
                        nCase.MicrotingUId = dbCase.MicrotingUid;
                        nCase.SiteId = dbCase.Site.MicrotingUid;
                        nCase.SiteName = dbCase.Site.Name;
                        nCase.Status = dbCase.Status;
                        nCase.TemplatId = dbCase.CheckListId;
                        nCase.UnitId = dbCase.Unit.MicrotingUid;
                        nCase.UpdatedAt = dbCase.UpdatedAt;
                        nCase.Version = dbCase.Version;
                        nCase.WorkerName = dbCase.Worker.FirstName + " " + dbCase.Worker.LastName;
                        nCase.WorkflowState = dbCase.WorkflowState;
                        nCase.FieldValue1 = dbCase.FieldValue1;
                        nCase.FieldValue2 = dbCase.FieldValue2;
                        nCase.FieldValue3 = dbCase.FieldValue3;
                        nCase.FieldValue4 = dbCase.FieldValue4;
                        nCase.FieldValue5 = dbCase.FieldValue5;
                        nCase.FieldValue6 = dbCase.FieldValue6;
                        nCase.FieldValue7 = dbCase.FieldValue7;
                        nCase.FieldValue8 = dbCase.FieldValue8;
                        nCase.FieldValue9 = dbCase.FieldValue9;
                        nCase.FieldValue10 = dbCase.FieldValue10;

                        rtrnLst.Add(nCase);
                    }
                    #endregion

                    CaseList caseList = new CaseList(numOfElements, pageSize, rtrnLst);
                    
                    
                    return caseList;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CaseReadFull failed", ex);
            }
        }

        /// <summary>
        /// Returns a list of Case objects from DB with given parameters as templateId, startTime, endTime, workflowstate, searchkey, descendingsort, sortparameter
        /// </summary>
        /// <param name="templatId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="workflowState"></param>
        /// <param name="searchKey"></param>
        /// <param name="descendingSort"></param>
        /// <param name="sortParameter"></param>
        /// <returns></returns>
        public async Task<List<Case>> CaseReadAll(int? templatId, DateTime? start, DateTime? end, string workflowState, string searchKey, bool descendingSort, string sortParameter)
        {            
            string methodName = t.GetMethodName("SQLController");
            await log.LogStandard(methodName, "called");
            await log.LogVariable(methodName, nameof(templatId), templatId);
            await log.LogVariable(methodName, nameof(start), start);
            await log.LogVariable(methodName, nameof(end), end);
            await log.LogVariable(methodName, nameof(workflowState), workflowState);
            await log.LogVariable(methodName, nameof(searchKey), searchKey);
            await log.LogVariable(methodName, nameof(descendingSort), descendingSort);
            await log.LogVariable(methodName, nameof(sortParameter), sortParameter);

            CaseList cl = await CaseReadAll(templatId, start, end, workflowState, searchKey, descendingSort, sortParameter, 0,
                10000000);
            
            List<Case> rtnCaseList = new List<Case>();
            
            foreach (Case @case in cl.Cases)
            {
                rtnCaseList.Add(@case);
            }

            return rtnCaseList;

        }

        /// <summary>
        /// Returns a list of case data transfer objects from DB with given Custom string
        /// </summary>
        /// <param name="customString"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<Case_Dto>> CaseFindCustomMatchs(string customString)
        {
            try
            {
                using (var db = GetContext())
                {
                    List<Case_Dto> foundCasesThatMatch = new List<Case_Dto>();

                    List<cases> lstMatchs = db.cases.Where(x => x.Custom == customString && x.WorkflowState == Constants.Constants.WorkflowStates.Created).ToList();

                    foreach (cases match in lstMatchs)
                        foundCasesThatMatch.Add(await CaseReadByCaseId(match.Id));

                    return foundCasesThatMatch;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CaseFindCustomMatchs failed", ex);
            }
        }

        //TODO
        public async Task<bool> CaseUpdateFieldValues(int caseId)
        {
            try
            {
                using (var db = GetContext())
                {
                    cases lstMatchs = await db.cases.SingleOrDefaultAsync(x => x.Id == caseId);

                    if (lstMatchs == null)
                        return false;

                    lstMatchs.UpdatedAt = DateTime.Now;
                    lstMatchs.Version = lstMatchs.Version + 1;
                    List<int?> case_fields = new List<int?>();
                    check_lists cl = lstMatchs.CheckList;

                    case_fields.Add(cl.Field1);
                    case_fields.Add(cl.Field2);
                    case_fields.Add(cl.Field3);
                    case_fields.Add(cl.Field4);
                    case_fields.Add(cl.Field5);
                    case_fields.Add(cl.Field6);
                    case_fields.Add(cl.Field7);
                    case_fields.Add(cl.Field8);
                    case_fields.Add(cl.Field9);
                    case_fields.Add(cl.Field10);

                    lstMatchs.FieldValue1 = null;
                    lstMatchs.FieldValue2 = null;
                    lstMatchs.FieldValue3 = null;
                    lstMatchs.FieldValue4 = null;
                    lstMatchs.FieldValue5 = null;
                    lstMatchs.FieldValue6 = null;
                    lstMatchs.FieldValue7 = null;
                    lstMatchs.FieldValue8 = null;
                    lstMatchs.FieldValue9 = null;
                    lstMatchs.FieldValue10 = null;

                    List<field_values> field_values = db.field_values.Where(x => x.CaseId == lstMatchs.Id && case_fields.Contains(x.FieldId)).ToList();

                    foreach (field_values item in field_values)
                    {
                        field_types field_type = item.Field.FieldType;
                        string new_value = item.Value;

                        if (field_type.FieldType == Constants.Constants.FieldTypes.EntitySearch || field_type.FieldType == Constants.Constants.FieldTypes.EntitySelect)
                        {
                            try
                            {
                                if (item.Value != "" || item.Value != null)
                                {
                                    entity_items match = await db.entity_items.SingleOrDefaultAsync(x => x.Id == int.Parse(item.Value));

                                    if (match != null)
                                    {
                                        new_value = match.Name;
                                    }

                                }
                            }
                            catch { }
                        }

                        if (field_type.FieldType == Constants.Constants.FieldTypes.SingleSelect)
                        {
                            string key = item.Value;
                            string fullKey = t.Locate(item.Field.KeyValuePairList, "<" + key + ">", "</" + key + ">");
                            new_value = t.Locate(fullKey, "<key>", "</key>");
                        }

                        if (field_type.FieldType == Constants.Constants.FieldTypes.MultiSelect)
                        {
                            new_value = "";

                            string keys = item.Value;
                            List<string> keyLst = keys.Split('|').ToList();

                            foreach (string key in keyLst)
                            {
                                string fullKey = t.Locate(item.Field.KeyValuePairList, "<" + key + ">", "</" + key + ">");
                                if (new_value != "")
                                {
                                    new_value += "\n" + t.Locate(fullKey, "<key>", "</key>");
                                }
                                else
                                {
                                    new_value += t.Locate(fullKey, "<key>", "</key>");
                                }
                            }
                        }


                        int i = case_fields.IndexOf(item.FieldId);
                        switch (i)
                        {
                            case 0:
                                lstMatchs.FieldValue1 = new_value;
                                break;
                            case 1:
                                lstMatchs.FieldValue2 = new_value;
                                break;
                            case 2:
                                lstMatchs.FieldValue3 = new_value;
                                break;
                            case 3:
                                lstMatchs.FieldValue4 = new_value;
                                break;
                            case 4:
                                lstMatchs.FieldValue5 = new_value;
                                break;
                            case 5:
                                lstMatchs.FieldValue6 = new_value;
                                break;
                            case 6:
                                lstMatchs.FieldValue7 = new_value;
                                break;
                            case 7:
                                lstMatchs.FieldValue8 = new_value;
                                break;
                            case 8:
                                lstMatchs.FieldValue9 = new_value;
                                break;
                            case 9:
                                lstMatchs.FieldValue10 = new_value;
                                break;
                        }
                    }
                    
                    await lstMatchs.Update(db);

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(t.GetMethodName("SQLController") + " failed", ex);
            }
        }
        #endregion
        
        #region public site
        #region site
        
        /// <summary>
        /// Returns a list of site data transfer objects from DB
        /// </summary>
        /// <param name="includeRemoved"></param>
        /// <returns></returns>
        public async Task<List<SiteName_Dto>> SiteGetAll(bool includeRemoved)
        {
            List<SiteName_Dto> siteList = new List<SiteName_Dto>();
            using (var db = GetContext())
            {
                List<sites> matches = null;
                if(includeRemoved)
                    matches = await db.sites.ToListAsync();
                else
                    matches = await db.sites.Where(x => x.WorkflowState == Constants.Constants.WorkflowStates.Created).ToListAsync();

                foreach (sites aSite in matches)
                {
                    SiteName_Dto siteNameDto = new SiteName_Dto((int)aSite.MicrotingUid, aSite.Name, aSite.CreatedAt, aSite.UpdatedAt);
                    siteList.Add(siteNameDto);
                }
            }
            return siteList;

        }

        //TODO
        public async Task<List<Site_Dto>> SimpleSiteGetAll(string workflowState, int? offSet, int? limit)
        {
            List<Site_Dto> siteList = new List<Site_Dto>();
            using (var db = GetContext())
            {
                List<sites> matches = null;
                switch (workflowState)
                {
                    case Constants.Constants.WorkflowStates.NotRemoved:
                        matches = await db.sites.Where(x => x.WorkflowState != Constants.Constants.WorkflowStates.Removed).ToListAsync();
                        break;
                    case Constants.Constants.WorkflowStates.Removed:
                        matches = await db.sites.Where(x => x.WorkflowState == Constants.Constants.WorkflowStates.Removed).ToListAsync();
                        break;
                    case Constants.Constants.WorkflowStates.Created:
                        matches = await db.sites.Where(x => x.WorkflowState == Constants.Constants.WorkflowStates.Created).ToListAsync();
                        break;
                    default:
                        matches = await db.sites.ToListAsync();
                        break;
                }
                foreach (sites aSite in matches)
                {
                    units unit = null;
                    workers worker = null;
                    int? unitCustomerNo = null;
                    int? unitOptCode = null;
                    int? unitMicrotingUid = null;
                    int? workerMicrotingUid = null;
                    string workerFirstName = null;
                    string workerLastName = null;
                    try
                    {
                        unit = aSite.Units.First();
                        unitCustomerNo = (int)unit.CustomerNo;
                        unitOptCode = (int)unit.OtpCode;
                        unitMicrotingUid = (int)unit.MicrotingUid;
                    }
                    catch { }

                    try
                    {
                        worker = aSite.SiteWorkers.First().Worker;
                        workerMicrotingUid = worker.MicrotingUid;
                        workerFirstName = worker.FirstName;
                        workerLastName = worker.LastName;
                    }
                    catch { }

                    try
                    {
                        Site_Dto siteDto = new Site_Dto((int)aSite.MicrotingUid, aSite.Name, workerFirstName, workerLastName, unitCustomerNo, unitOptCode, unitMicrotingUid, workerMicrotingUid);
                        siteList.Add(siteDto);
                    }
                    catch
                    {
                        Site_Dto siteDto = new Site_Dto((int)aSite.MicrotingUid, aSite.Name, workerFirstName, workerLastName, unitCustomerNo, unitOptCode, unitMicrotingUid, workerMicrotingUid);
                        siteList.Add(siteDto);
                    }
                }
            }
            return siteList;

        }

        /// <summary>
        /// Creates a site in DB with given microtingUid and Name
        /// </summary>
        /// <param name="microtingUid"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> SiteCreate(int microtingUid, string name)
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                using (var db = GetContext())
                {
                    //logger.LogEverything(methodName + " called");
                    //logger.LogEverything("siteName:" + siteName + " / userFirstName:" + userFirstName + " / userLastName:" + userLastName);

                    sites site = new sites();
//                    site.WorkflowState = Constants.Constants.WorkflowStates.Created;
//                    site.Version = 1;
//                    site.CreatedAt = DateTime.Now;
//                    site.UpdatedAt = DateTime.Now;
                    site.MicrotingUid = microtingUid;
                    site.Name = name;
                    await site.Create(db);


//                    db.sites.Add(site);
//                    db.SaveChanges();

//                    db.site_versions.Add(MapSiteVersions(site));
//                    db.SaveChanges();

                    return site.Id;
                }
            }
            catch (Exception ex)
            {
                //logger.LogException(methodName + " failed", ex, true);
                throw new Exception(methodName + " failed", ex);
            }
        }

        /// <summary>
        /// Reads a site from DB with given microting_uid
        /// </summary>
        /// <param name="microting_uid"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<SiteName_Dto> SiteRead(int microting_uid)
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                using (var db = GetContext())
                {
                    sites site = await db.sites.SingleOrDefaultAsync(x => x.MicrotingUid == microting_uid && x.WorkflowState == Constants.Constants.WorkflowStates.Created);

                    if (site != null)
                        return new SiteName_Dto((int)site.MicrotingUid, site.Name, site.CreatedAt, site.UpdatedAt);
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(methodName + " failed", ex);
            }
        }

        //TODO
        public async Task<Site_Dto> SiteReadSimple(int microting_uid)
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                using (var db = GetContext())
                {
                    sites site = await db.sites.SingleOrDefaultAsync(x => x.MicrotingUid == microting_uid && x.WorkflowState == Constants.Constants.WorkflowStates.Created);
                    if (site == null)
                        return null;

                    site_workers site_worker = db.site_workers.Where(x => x.SiteId == site.Id).ToList().First();
                    workers worker = await db.workers.SingleAsync(x => x.Id == site_worker.WorkerId);
                    List<units> units = db.units.Where(x => x.SiteId == site.Id).ToList();

                    if (units.Count() > 0 && worker != null)
                    {
                        units unit = units.First();
                        return new Site_Dto((int)site.MicrotingUid, site.Name, worker.FirstName, worker.LastName, (int)unit.CustomerNo, (int)unit.OtpCode, (int)unit.MicrotingUid, worker.MicrotingUid);
                    }
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(methodName + " failed", ex);
            }
        }

        /// <summary>
        /// Updates site in DB with new values of microting_uid and name
        /// </summary>
        /// <param name="microting_uid"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> SiteUpdate(int microting_uid, string name)
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                using (var db = GetContext())
                {
                    //logger.LogEverything(methodName + " called");
                    //logger.LogEverything("siteName:" + siteName + " / userFirstName:" + userFirstName + " / userLastName:" + userLastName);

                    sites site = await db.sites.SingleOrDefaultAsync(x => x.MicrotingUid == microting_uid);

                    if (site != null)
                    {
//                        site.Version = site.Version + 1;
//                        site.UpdatedAt = DateTime.Now;

                        site.Name = name;
                        await site.Update(db);

//                        db.site_versions.Add(MapSiteVersions(site));
//                        db.SaveChanges();

                        return true;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                //logger.LogException(methodName + " failed", ex, true);
                throw new Exception(methodName + " failed", ex);
            }
        }

        /// <summary>
        /// Deletes site in DB with given microting_uid
        /// </summary>
        /// <param name="microting_uid"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> SiteDelete(int microting_uid)
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                using (var db = GetContext())
                {
                    //logger.LogEverything(methodName + " called");
                    //logger.LogEverything("siteName:" + siteName + " / userFirstName:" + userFirstName + " / userLastName:" + userLastName);

                    sites site = await db.sites.SingleOrDefaultAsync(x => x.MicrotingUid == microting_uid);

                    if (site != null)
                    {
                        await site.Delete(db);
//                        site.Version = site.Version + 1;
//                        site.UpdatedAt = DateTime.Now;

//                        site.WorkflowState = Constants.Constants.WorkflowStates.Removed;

//                        db.site_versions.Add(MapSiteVersions(site));
//                        db.SaveChanges();

                        return true;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                //logger.LogException(methodName + " failed", ex, true);
                throw new Exception(methodName + " failed", ex);
            }
        }
        #endregion

        #region worker
        
        /// <summary>
        /// Returns a list of workers from DB 
        /// </summary>
        /// <param name="workflowState"></param>
        /// <param name="offSet"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<Worker_Dto>> WorkerGetAll(string workflowState, int? offSet, int? limit)
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                List<Worker_Dto> listWorkerDto = new List<Worker_Dto>();

                using (var db = GetContext())
                {
                    List<workers> matches = null;

                    switch (workflowState)
                    {
                        case Constants.Constants.WorkflowStates.NotRemoved:
                            matches = await db.workers.Where(x => x.WorkflowState != Constants.Constants.WorkflowStates.Removed).ToListAsync();
                            break;
                        case Constants.Constants.WorkflowStates.Removed:
                            matches = await db.workers.Where(x => x.WorkflowState == Constants.Constants.WorkflowStates.Removed).ToListAsync();
                            break;
                        case Constants.Constants.WorkflowStates.Created:
                            matches = await db.workers.Where(x => x.WorkflowState == Constants.Constants.WorkflowStates.Created).ToListAsync();
                            break;
                        default:
                            matches = await db.workers.ToListAsync();
                            break;
                    }
                    foreach (workers worker in matches)
                    {
                        Worker_Dto workerDto = new Worker_Dto(worker.MicrotingUid, worker.FirstName, worker.LastName, worker.Email, worker.CreatedAt, worker.UpdatedAt);
                        listWorkerDto.Add(workerDto);
                    }
                    return listWorkerDto;
                }
            }
            catch (Exception ex)
            {
                //logger.LogException(methodName + " failed", ex, true);
                throw new Exception(methodName + " failed", ex);
            }

        }

        /// <summary>
        /// Creates a Worker in DB with given microtingUid, first name, last name and email.
        /// </summary>
        /// <param name="microtingUid"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> WorkerCreate(int microtingUid, string firstName, string lastName, string email)
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                using (var db = GetContext())
                {
                    //logger.LogEverything(methodName + " called");
                    workers worker = new workers();
                    worker.MicrotingUid = microtingUid;
                    worker.FirstName = firstName;
                    worker.LastName = lastName;
                    worker.Email = email;
                    await worker.Create(db);

                    return worker.Id;
                }
            }
            catch (Exception ex)
            {
                //logger.LogException(methodName + " failed", ex, true);
                throw new Exception(methodName + " failed", ex);
            }
        }

        /// <summary>
        /// Reads a workers name from DB with give workerId
        /// </summary>
        /// <param name="workerId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> WorkerNameRead(int workerId)
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                using (var db = GetContext())
                {
                    workers worker = await db.workers.SingleOrDefaultAsync(x => x.Id == workerId && x.WorkflowState == Constants.Constants.WorkflowStates.Created);

                    if (worker == null)
                        return null;
                    else
                        return worker.FirstName + " " + worker.LastName;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(methodName + " failed", ex);
            }
        }

        /// <summary>
        /// returns a worker data transfer object from DB with given microting_uid
        /// </summary>
        /// <param name="microting_uid"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Worker_Dto> WorkerRead(int microting_uid)
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                using (var db = GetContext())
                {
                    workers worker = await db.workers.SingleOrDefaultAsync(x => x.MicrotingUid == microting_uid && x.WorkflowState == Constants.Constants.WorkflowStates.Created);

                    if (worker != null)
                        return new Worker_Dto((int)worker.MicrotingUid, worker.FirstName, worker.LastName, worker.Email, worker.CreatedAt, worker.UpdatedAt);
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(methodName + " failed", ex);
            }
        }

        /// <summary>
        /// Updates worker in DB with new values of first name, last name and email.
        /// </summary>
        /// <param name="microtingUid"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> WorkerUpdate(int microtingUid, string firstName, string lastName, string email)
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                using (var db = GetContext())
                {
                    //logger.LogEverything(methodName + " called");
                    //logger.LogEverything("siteName:" + siteName + " / userFirstName:" + userFirstName + " / userLastName:" + userLastName);

                    workers worker = await db.workers.SingleOrDefaultAsync(x => x.MicrotingUid == microtingUid);

                    if (worker != null)
                    {
                        worker.FirstName = firstName;
                        worker.LastName = lastName;
                        worker.Email = email;
                        await worker.Update(db);
                        return true;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                //logger.LogException(methodName + " failed", ex, true);
                throw new Exception(methodName + " failed", ex);
            }
        }
        
        
        /// <summary>
        /// Deletes a worker with given microtingUid
        /// </summary>
        /// <param name="microtingUid"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> WorkerDelete(int microtingUid)
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                using (var db = GetContext())
                {
                    //logger.LogEverything(methodName + " called");

                    workers worker = await db.workers.SingleOrDefaultAsync(x => x.MicrotingUid == microtingUid);

                    if (worker != null)
                    {
                        await worker.Delete(db);
                        return true;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                //logger.LogException(methodName + " failed", ex, true);
                throw new Exception(methodName + " failed", ex);
            }
        }
        #endregion

        #region site_worker
        
        /// <summary>
        /// Creates Site Worker in DB with given microtingUId, siteUId and workerUId
        /// </summary>
        /// <param name="microtingUId"></param>
        /// <param name="siteUId"></param>
        /// <param name="workerUId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> SiteWorkerCreate(int microtingUId, int siteUId, int workerUId)
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                using (var db = GetContext())
                {
                    //logger.LogEverything(methodName + " called");

                    int localSiteId = db.sites.SingleAsync(x => x.MicrotingUid == siteUId).Result.Id;
                    int localWorkerId = db.workers.SingleAsync(x => x.MicrotingUid == workerUId).Result.Id;

                    site_workers siteWorker = new site_workers();
//                    site_worker.WorkflowState = Constants.Constants.WorkflowStates.Created;
//                    site_worker.Version = 1;
//                    site_worker.CreatedAt = DateTime.Now;
//                    site_worker.UpdatedAt = DateTime.Now;
                    siteWorker.MicrotingUid = microtingUId;
                    siteWorker.SiteId = localSiteId;
                    siteWorker.WorkerId = localWorkerId;
                    await siteWorker.Create(db);


//                    db.site_workers.Add(site_worker);
//                    db.SaveChanges();

//                    db.site_worker_versions.Add(MapSiteWorkerVersions(site_worker));
//                    db.SaveChanges();

                    return siteWorker.Id;
                }
            }
            catch (Exception ex)
            {
                //logger.LogException(methodName + " failed", ex, true);
                throw new Exception(methodName + " failed", ex);
            }
        }

        /// <summary>
        /// Reads Site Worker from DB 
        /// </summary>
        /// <param name="siteWorkerMicrotingUid"></param>
        /// <param name="siteId"></param>
        /// <param name="workerId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Site_Worker_Dto> SiteWorkerRead(int? siteWorkerMicrotingUid, int? siteId, int? workerId)
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                using (var db = GetContext())
                {
                    //logger.LogEverything(methodName + " called");
                    site_workers siteWorker = null;
                    if (siteWorkerMicrotingUid == null)
                    {
                        sites site = await db.sites.SingleAsync(x => x.MicrotingUid == siteId);
                        workers worker = await db.workers.SingleAsync(x => x.MicrotingUid == workerId);
                        siteWorker = await db.site_workers.SingleOrDefaultAsync(x => x.SiteId == site.Id && x.WorkerId == worker.Id);
                    }
                    else
                    {
                        siteWorker = await db.site_workers.SingleOrDefaultAsync(x => x.MicrotingUid == siteWorkerMicrotingUid && x.WorkflowState == Constants.Constants.WorkflowStates.Created);
                    }


                    if (siteWorker != null)
                        return new Site_Worker_Dto((int)siteWorker.MicrotingUid, (int)siteWorker.Site.MicrotingUid, (int)siteWorker.Worker.MicrotingUid);
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                //logger.LogException(methodName + " failed", ex, true);
                throw new Exception(methodName + " failed", ex);
            }
        }

        /// <summary>
        /// Updates Site Worker with new values of microtingUid, siteId and workerId
        /// </summary>
        /// <param name="microtingUid"></param>
        /// <param name="siteId"></param>
        /// <param name="workerId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> SiteWorkerUpdate(int microtingUid, int siteId, int workerId)
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                using (var db = GetContext())
                {
                    //logger.LogEverything(methodName + " called");

                    site_workers site_worker = await db.site_workers.SingleOrDefaultAsync(x => x.MicrotingUid == microtingUid);

                    if (site_worker != null)
                    {
//                        site_worker.Version = site_worker.Version + 1;
//                        site_worker.UpdatedAt = DateTime.Now;

                        site_worker.SiteId = siteId;
                        site_worker.WorkerId = workerId;
                        await site_worker.Update(db);

//                        db.site_worker_versions.Add(MapSiteWorkerVersions(site_worker));
//                        db.SaveChanges();


                        return true;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                //logger.LogException(methodName + " failed", ex, true);
                throw new Exception(methodName + " failed", ex);
            }
        }

        /// <summary>
        /// Deletes Site Worker from DB with given micrioting_uid
        /// </summary>
        /// <param name="microting_uid"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> SiteWorkerDelete(int microting_uid)
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                using (var db = GetContext())
                {
                    //logger.LogEverything(methodName + " called");

                    site_workers site_worker = await db.site_workers.SingleOrDefaultAsync(x => x.MicrotingUid == microting_uid);

                    if (site_worker != null)
                    {
                        await site_worker.Delete(db);
//                        site_worker.Version = site_worker.Version + 1;
//                        site_worker.UpdatedAt = DateTime.Now;

//                        site_worker.WorkflowState = Constants.Constants.WorkflowStates.Removed;

//                        db.site_worker_versions.Add(MapSiteWorkerVersions(site_worker));
//                        db.SaveChanges();

                        return true;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                //logger.LogException(methodName + " failed", ex, true);
                throw new Exception(methodName + " failed", ex);
            }
        }
        #endregion

        #region unit
        
        /// <summary>
        /// Returns list of unit data transfer objects from DB
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<Unit_Dto>> UnitGetAll()
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                List<Unit_Dto> listWorkerDto = new List<Unit_Dto>();
                using (var db = GetContext())
                {
                    foreach (units unit in await db.units.ToListAsync())
                    {
                        Unit_Dto unitDto = new Unit_Dto((int)unit.MicrotingUid, (int)unit.CustomerNo, (int)unit.OtpCode, (int)unit.Site.MicrotingUid, unit.CreatedAt, unit.UpdatedAt);
                        listWorkerDto.Add(unitDto);
                    }
                }
                return listWorkerDto;
            }
            catch (Exception ex)
            {
                //logger.LogException(methodName + " failed", ex, true);
                throw new Exception(methodName + " failed", ex);
            }
        }
        
        
        /// <summary>
        /// Creates Unit in DB with given micriotingUid, customer number, one time password and siteUId
        /// </summary>
        /// <param name="microtingUid"></param>
        /// <param name="customerNo"></param>
        /// <param name="otpCode"></param>
        /// <param name="siteUId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> UnitCreate(int microtingUid, int customerNo, int otpCode, int siteUId)
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                using (var db = GetContext())
                {
                    //logger.LogEverything(methodName + " called");
                    int localSiteId = db.sites.SingleAsync(x => x.MicrotingUid == siteUId).Result.Id;

                    units unit = new units();
                    unit.MicrotingUid = microtingUid;
                    unit.CustomerNo = customerNo;
                    unit.OtpCode = otpCode;
                    unit.SiteId = localSiteId;

                    await unit.Create(db);

                    return unit.Id;
                }
            }
            catch (Exception ex)
            {
                //logger.LogException(methodName + " failed", ex, true);
                throw new Exception(methodName + " failed", ex);
            }
        }

        /// <summary>
        /// Reads Unit from DB with given microtingUid
        /// </summary>
        /// <param name="microtingUid"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Unit_Dto> UnitRead(int microtingUid)
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                using (var db = GetContext())
                {
                    //logger.LogEverything(methodName + " called");

                    units unit = await db.units.SingleOrDefaultAsync(x => x.MicrotingUid == microtingUid && x.WorkflowState == Constants.Constants.WorkflowStates.Created);

                    if (unit != null)
                        return new Unit_Dto((int)unit.MicrotingUid, (int)unit.CustomerNo, (int)unit.OtpCode, (int)unit.SiteId, unit.CreatedAt, unit.UpdatedAt);
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                //logger.LogException(methodName + " failed", ex, true);
                throw new Exception(methodName + " failed", ex);
            }
        }

        /// <summary>
        /// Updates Unit in DB with new values
        /// </summary>
        /// <param name="microtingUid"></param>
        /// <param name="customerNo"></param>
        /// <param name="otpCode"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> UnitUpdate(int microtingUid, int customerNo, int otpCode, int siteId)
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                using (var db = GetContext())
                {
                    //logger.LogEverything(methodName + " called");

                    units unit = await db.units.SingleOrDefaultAsync(x => x.MicrotingUid == microtingUid);

                    if (unit != null)
                    {
                        unit.CustomerNo = customerNo;
                        unit.OtpCode = otpCode;
                        await unit.Update(db);

                        return true;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                //logger.LogException(methodName + " failed", ex, true);
                throw new Exception(methodName + " failed", ex);
            }
        }
        
        /// <summary>
        /// Deletes Unit from DB with given microtingUid
        /// </summary>
        /// <param name="microtingUid"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> UnitDelete(int microtingUid)
        {
            string methodName = t.GetMethodName("SQLController");
            try
            {
                using (var db = GetContext())
                {
                    //logger.LogEverything(methodName + " called");

                    units unit = await db.units.SingleOrDefaultAsync(x => x.MicrotingUid == microtingUid);

                    if (unit != null)
                    {
                        await unit.Delete(db);

                        return true;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                //logger.LogException(methodName + " failed", ex, true);
                throw new Exception(methodName + " failed", ex);
            }
        }
        #endregion
        #endregion

        #region public entity
        #region entityGroup
        
        //TODO
        public async Task<EntityGroupList> EntityGroupAll(string sort, string nameFilter, int offSet, int pageSize, string entityType, bool desc, string workflowState)
        {

            if (entityType != Constants.Constants.FieldTypes.EntitySearch && entityType != Constants.Constants.FieldTypes.EntitySelect)
                throw new Exception("EntityGroupAll failed. EntityType:" + entityType + " is not an known type");
            if (workflowState != Constants.Constants.WorkflowStates.NotRemoved && workflowState != Constants.Constants.WorkflowStates.Created && workflowState != Constants.Constants.WorkflowStates.Removed)
                throw new Exception("EntityGroupAll failed. workflowState:" + workflowState + " is not an known workflow state");

            List<entity_groups> eG = null;
            List<EntityGroup> e_G = new List<EntityGroup>();
            int numOfElements = 0;
            try
            {
                using (var db = GetContext())
                {
                    var source = db.entity_groups.Where(x => x.Type == entityType);
                    if (nameFilter != "")
                        source = source.Where(x => x.Name.Contains(nameFilter));
                    if (sort == "Id")
                        if (desc)
                            source = source.OrderByDescending(x => x.Id);
                        else
                            source = source.OrderBy(x => x.Id);
                    else
                        if (desc)
                        source = source.OrderByDescending(x => x.Name);
                    else
                        source = source.OrderBy(x => x.Id);

                    switch (workflowState)
                    {
                        case Constants.Constants.WorkflowStates.NotRemoved:
                            source = source.Where(x => x.WorkflowState != Constants.Constants.WorkflowStates.Removed);
                            break;
                        case Constants.Constants.WorkflowStates.Removed:
                            source = source.Where(x => x.WorkflowState == Constants.Constants.WorkflowStates.Removed);
                            break;
                        case Constants.Constants.WorkflowStates.Created:
                            source = source.Where(x => x.WorkflowState == Constants.Constants.WorkflowStates.Created);
                            break;
                    }

                    numOfElements = source.Count();
                    if (numOfElements < pageSize)
                    {
                        eG = await source.ToListAsync();
                    }
                    else
                    {
                        eG = await source.Skip(offSet).Take(pageSize).ToListAsync();
                    }

                    foreach (entity_groups eg in eG)
                    {
//                        EntityGroup g = new EntityGroup(eg.Id, eg.Name, eg.Type, eg.MicrotingUid, new List<EntityItem>(), eg.WorkflowState, eg.CreatedAt, eg.UpdatedAt);
                        EntityGroup g = new EntityGroup()
                        {
                            Id = eg.Id,
                            Name = eg.Name,
                            Type = eg.Type,
                            MicrotingUUID = eg.MicrotingUid,
                            EntityGroupItemLst = new List<EntityItem>(),
                            WorkflowState = eg.WorkflowState,
                            CreatedAt = eg.CreatedAt,
                            UpdatedAt = eg.UpdatedAt
                        };
                        e_G.Add(g);
                    }
                    return new EntityGroupList(numOfElements, offSet, e_G);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("EntityGroupAll failed", ex);
            }
        }

        /// <summary>
        /// Crates an Entity Group in DB with given name and type of entity
        /// </summary>
        /// <param name="name"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<EntityGroup> EntityGroupCreate(string name, string entityType)
        {
            try
            {
                if (entityType != Constants.Constants.FieldTypes.EntitySearch && entityType != Constants.Constants.FieldTypes.EntitySelect)
                    throw new Exception("EntityGroupCreate failed. EntityType:" + entityType + " is not an known type");

                using (var db = GetContext())
                {
                    entity_groups eG = new entity_groups {Name = name, Type = entityType};

                    await eG.Create(db);

                    return new EntityGroup
                    {
                        Id = eG.Id,
                        Name = eG.Name,
                        Type = eG.Type,
                        MicrotingUUID = eG.MicrotingUid,
                        EntityGroupItemLst = new List<EntityItem>(),
                        WorkflowState = eG.WorkflowState,
                        CreatedAt = eG.CreatedAt,
                        UpdatedAt = eG.UpdatedAt
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception("EntityGroupCreate failed", ex);
            }
        }

        //TODO
        public async Task<EntityGroup> EntityGroupReadSorted(string entityGroupMUId, string sort, string nameFilter)
        {
            try
            {
                using (var db = GetContext())
                {
                    entity_groups eG = await db.entity_groups.SingleOrDefaultAsync(x => x.MicrotingUid == entityGroupMUId);

                    if (eG == null)
                        return null;

                    List<EntityItem> lst = new List<EntityItem>();
//                    EntityGroup rtnEG = new EntityGroup(eG.Id, eG.Name, eG.Type, eG.MicrotingUid, lst, eG.WorkflowState, eG.CreatedAt, eG.UpdatedAt);
                    EntityGroup rtnEG = new EntityGroup
                    {
                        Id = eG.Id,
                        Name = eG.Name,
                        Type = eG.Type,
                        MicrotingUUID = eG.MicrotingUid,
                        EntityGroupItemLst = lst,
                        WorkflowState = eG.WorkflowState,
                        CreatedAt = eG.CreatedAt,
                        UpdatedAt = eG.UpdatedAt
                    };

                    List<entity_items> eILst = null;

                    if (string.IsNullOrEmpty(nameFilter))
                    {
                        eILst = db.entity_items.
                            Where(x => x.EntityGroupId == eG.Id 
                                       && x.WorkflowState != Constants.Constants.WorkflowStates.Removed 
                                       && x.WorkflowState != Constants.Constants.WorkflowStates.FailedToSync).
                            CustomOrderBy(sort).ToList();
                    }
                    else
                    {
                        eILst = db.entity_items.
                            Where(x => x.EntityGroupId == eG.Id 
                                       && x.WorkflowState != Constants.Constants.WorkflowStates.Removed 
                                       && x.WorkflowState != Constants.Constants.WorkflowStates.FailedToSync 
                                       && x.Name.Contains(nameFilter)).
                            CustomOrderBy(sort).ToList();
                    }

                    if (eILst.Count > 0)
                        foreach (entity_items item in eILst)
                        {
                            EntityItem eI = new EntityItem
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Description = item.Description,
                                EntityItemUId = item.EntityItemUid,
                                MicrotingUUID = item.MicrotingUid,
                                WorkflowState = item.WorkflowState,
                                DisplayIndex = item.DisplayIndex
                            };
                            lst.Add(eI);
                        }

                    return rtnEG;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("EntityGroupRead failed", ex);
            }
        }

        /// <summary>
        /// Reads Entity Group from DB with given Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<EntityGroup> EntityGroupRead(int Id) 
        {
            using (var db = GetContext()) {
                entity_groups eg = await db.entity_groups.SingleOrDefaultAsync(x => x.Id == Id);
                if (eg != null) {
                    List<EntityItem> egl = new List<EntityItem>();
                    return new EntityGroup
                    {
                        Id = eg.Id,
                        Name = eg.Name,
                        Type = eg.Type,
                        MicrotingUUID = eg.MicrotingUid,
                        EntityGroupItemLst = egl
                    };
//                    return new EntityGroup(eg.Id, eg.Name, eg.Type, eg.MicrotingUid, egl);
                } else {
                    throw new NullReferenceException("No EntityGroup found by Id " + Id.ToString());
                }
            }
        }

        //TODO
        public async Task<EntityGroup> EntityGroupRead(string entityGroupMUId)
        {
            return await EntityGroupReadSorted(entityGroupMUId, "Id", "");
        }

        /// <summary>
        /// Updates Entity Group in DB with given EntitygroupId and entityGroupMUId
        /// </summary>
        /// <param name="entityGroupId"></param>
        /// <param name="entityGroupMUId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> EntityGroupUpdate(int entityGroupId, string entityGroupMUId)
        {
            try
            {
                using (var db = GetContext())
                {
                    entity_groups eG = await db.entity_groups.SingleOrDefaultAsync(x => x.Id == entityGroupId);

                    if (eG == null)
                        return false;

                    eG.MicrotingUid = entityGroupMUId;
                    await eG.Update(db);

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("EntityGroupUpdate failed", ex);
            }
        }

        /// <summary>
        /// Updates Entity Group Name in DB with given name and entitygroupMUId
        /// </summary>
        /// <param name="name"></param>
        /// <param name="entityGroupMUId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> EntityGroupUpdateName(string name, string entityGroupMUId)
        {
            try
            {
                using (var db = GetContext())
                {
                    entity_groups eG = await db.entity_groups.SingleOrDefaultAsync(x => x.MicrotingUid == entityGroupMUId);

                    if (eG == null)
                        return false;

                    eG.Name = name;
                    await eG.Update(db);

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("EntityGroupUpdate failed", ex);
            }
        }

        /// <summary>
        /// Deletes Entity Group in DB with given entitygROUPMUId
        /// </summary>
        /// <param name="entityGroupMUId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> EntityGroupDelete(string entityGroupMUId)
        {
            try
            {
                using (var db = GetContext())
                {
                    List<string> killLst = new List<string>();

                    entity_groups eG = await db.entity_groups.SingleOrDefaultAsync(x => x.MicrotingUid == entityGroupMUId && x.WorkflowState != Constants.Constants.WorkflowStates.Removed);

                    if (eG == null)
                        return null;

                    killLst.Add(eG.MicrotingUid);

                    await eG.Delete(db);

                    List<entity_items> lst = db.entity_items.Where(x => x.EntityGroupId == eG.Id && x.WorkflowState != Constants.Constants.WorkflowStates.Removed).ToList();
                    if (lst != null)
                    {
                        foreach (entity_items item in lst)
                        {
                            item.Synced = t.Bool(false);
                            await item.Update(db);
                            await item.Delete(db);

                            killLst.Add(item.MicrotingUid);
                        }
                    }

                    return eG.Type;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("EntityGroupDelete failed", ex);
            }
        }
        #endregion

        #region entityItem 
        
        /// <summary>
        /// Reads an entity item from DB with given microtingUid
        /// </summary>
        /// <param name="microtingUId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<entity_items> EntityItemRead(string microtingUId)
        {
            try
            {
                using (var db = GetContext())
                {
                    return await db.entity_items.SingleAsync(x => x.MicrotingUid == microtingUId);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("EntityItemRead failed", ex);
            }
        }

        /// <summary>
        /// Reads an Entity Item from DB with given Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<EntityItem> EntityItemRead(int id)
        {
            using (var db = GetContext())
            {
                entity_items et = await db.entity_items.FirstOrDefaultAsync(x => x.Id == id);
                if (et != null)
                {
                    EntityItem entityItem = new EntityItem
                    {
                        Id = et.Id,
                        Name = et.Name,
                        Description = et.Description,
                        EntityItemUId = et.EntityItemUid,
                        MicrotingUUID = et.MicrotingUid,
                        DisplayIndex = et.DisplayIndex,
                        EntityItemGroupId = et.EntityGroupId,
                        WorkflowState = et.WorkflowState
                    };
//                    entityItem.EntityItemGroupId = et.EntityGroupId;
//                    entityItem.Id = et.Id;
                    return entityItem;
                }
                else
                {
                    throw new NullReferenceException("No EntityItem found for Id " + id.ToString());
                }
            }
        }

        
        /// <summary>
        /// Reads an entity item group with given id, name and description from DB
        /// </summary>
        /// <param name="entityItemGroupId"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public async Task<EntityItem> EntityItemRead(int entityItemGroupId, string name, string description)
        {
            using (var db = GetContext())
            {
                entity_items et = await db.entity_items.SingleOrDefaultAsync(x => x.Name == name 
                                                                       && x.Description == description 
                                                                       && x.EntityGroupId == entityItemGroupId);
                if (et != null)
                {
                    return new EntityItem
                    {
                        Id = et.Id,
                        Name = et.Name,
                        Description = et.Description,
                        EntityItemUId = et.EntityItemUid,
                        MicrotingUUID = et.MicrotingUid,
                        WorkflowState = et.WorkflowState
                    };
                }
                else
                {
                    return null;
                }
            }
        }

    
        //TODO
        public async Task<EntityItem> EntityItemCreate(int entityItemGroupId, EntityItem entityItem)
        {

            using (var db = GetContext())
            {
                entity_items eI = new entity_items();
                eI.EntityGroupId = entityItemGroupId;
                eI.EntityItemUid = entityItem.EntityItemUId;
                eI.MicrotingUid = entityItem.MicrotingUUID;
                eI.Name = entityItem.Name;
                eI.Description = entityItem.Description;
                eI.DisplayIndex = entityItem.DisplayIndex;
                eI.Synced = t.Bool(false);
                await eI.Create(db);
            }
            return entityItem;
        }

        /// <summary>
        /// Updates an Entity Item in DB
        /// </summary>
        /// <param name="entityItem"></param>
        public async Task EntityItemUpdate(EntityItem entityItem)
        {
            using (var db = GetContext())
            {
                var match = await db.entity_items.SingleOrDefaultAsync(x => x.Id == entityItem.Id);
                match.Description = entityItem.Description;
                match.Name = entityItem.Name;
                match.Synced = t.Bool(false);
                match.EntityItemUid = entityItem.EntityItemUId;
                match.WorkflowState = entityItem.WorkflowState;
                match.DisplayIndex = entityItem.DisplayIndex;
                await match.Update(db);
            }
        }

        /// <summary>
        /// Deletes an Entity Item in DB with given Id
        /// </summary>
        /// <param name="Id"></param>
        /// <exception cref="NullReferenceException"></exception>
        public async Task EntityItemDelete(int Id)
        {
            using (var db = GetContext())
            {
                entity_items et = await db.entity_items.SingleOrDefaultAsync(x => x.Id == Id);
                if (et == null)
                {
                    throw new NullReferenceException("EntityItem not found with Id " + Id.ToString());
                }
                else
                {
                    et.Synced = t.Bool(true);
                    await et.Update(db);
                    await et.Delete(db);
                }
            }
        }
        #endregion
        #endregion

        #region folders

        /// <summary>
        /// Returns a list of Folders of type of folder data transfer objects from DB
        /// </summary>
        /// <param name="includeRemoved"></param>
        /// <returns></returns>
        public async Task<List<Folder_Dto>> FolderGetAll(bool includeRemoved)
        {
            List<Folder_Dto> folderDtos = new List<Folder_Dto>();
            using (var db = GetContext())
            {
                List<folders> matches = null;
                matches = includeRemoved ? await db.folders.ToListAsync() : await db.folders.Where(x => x.WorkflowState == Constants.Constants.WorkflowStates.Created).ToListAsync();

                foreach (folders folder in matches)
                {
                    Folder_Dto folderDto = new Folder_Dto(folder.Id, folder.Name, folder.Description, folder.ParentId, folder.CreatedAt, folder.UpdatedAt, folder.MicrotingUid);
                    folderDtos.Add(folderDto);
                }
            }

            return folderDtos;
        }

        /// <summary>
        /// Reads a Folder of type Folder data transfer object from DB with specific Microting_UID
        /// </summary>
        /// <param name="microting_uid"></param>
        /// <returns></returns>
        public async Task<Folder_Dto> FolderReadByMicrotingUUID(int microting_uid)
        {
            using (var db = GetContext())
            {
                folders folder = await db.folders.SingleOrDefaultAsync(x => x.MicrotingUid == microting_uid);

                if (folder == null)
                {
                    return null;
                }

                Folder_Dto folderDto = new Folder_Dto(folder.Id, folder.Name, folder.Description, folder.ParentId, folder.CreatedAt, folder.UpdatedAt, folder.MicrotingUid);
                return folderDto;
            }
        }

        /// <summary>
        /// Reads a folder from DB with given ID
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<Folder_Dto> FolderRead(int Id)
        {
            using (var db = GetContext())
            {
                folders folder = await db.folders.SingleOrDefaultAsync(x => x.Id == Id);

                if (folder == null)
                {
                    throw new NullReferenceException($"Could not find area with Id: {Id}");
                }

                Folder_Dto folderDto = new Folder_Dto(folder.Id, folder.Name, folder.Description, folder.ParentId, folder.CreatedAt, folder.UpdatedAt, folder.MicrotingUid);
                return folderDto;
            }
        }

        /// <summary>
        /// Creates a folder in DB with given name, description, parent id and microtingUUID
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="parent_id"></param>
        /// <param name="microtingUUID"></param>
        /// <returns></returns>
        public async Task<int> FolderCreate(string name, string description, int? parent_id, int microtingUUID)
        {
            folders folder = new folders
            {
                Name = name,
                Description = description,
                ParentId = parent_id,
                MicrotingUid = microtingUUID
            };

            await folder.Create(GetContext());
            
            return folder.Id;
        }

        /// <summary>
        /// Updates Folder in DB with new values of Id, Name, Description and Parent Id
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="parent_id"></param>
        /// <exception cref="NullReferenceException"></exception>
        public async Task FolderUpdate(int Id, string name, string description, int? parent_id)
        {
            using (var db = GetContext())
            {
                folders folder = await db.folders.SingleOrDefaultAsync(x => x.Id == Id);

                if (folder == null)
                {
                    throw new NullReferenceException($"Could not find area with Id: {Id}");
                }

                folder.Name = name;
                folder.Description = description;
                folder.ParentId = parent_id;

                await folder.Update(db);
            }
        }

        /// <summary>
        /// Deletes a Folder in DB with given ID
        /// </summary>
        /// <param name="Id"></param>
        /// <exception cref="NullReferenceException"></exception>
        public async Task FolderDelete(int Id)
        {
            using (var db = GetContext())
            {
                folders folder = await db.folders.SingleOrDefaultAsync(x => x.Id == Id);

                if (folder == null)
                {
                    throw new NullReferenceException($"Could not find area with Id: {Id}");
                }

                await folder.Delete(db);
            }
        }
        
        #endregion
        
        
        #region public setting
        
        //TODO
        public async Task<bool> SettingCreateDefaults()
        {
            WriteDebugConsoleLogEntry(new LogEntry(2, "SqlController.SettingCreateDefaults", "called"));
            //key point
            await SettingCreate(Settings.firstRunDone);
            await SettingCreate(Settings.logLevel);
            await SettingCreate(Settings.logLimit);
            await SettingCreate(Settings.knownSitesDone);
            await SettingCreate(Settings.fileLocationPicture);
            await SettingCreate(Settings.fileLocationPdf);
            await SettingCreate(Settings.fileLocationJasper);
            await SettingCreate(Settings.token);
            await SettingCreate(Settings.comAddressBasic);
            await SettingCreate(Settings.comAddressApi);
            await SettingCreate(Settings.comAddressPdfUpload);
            await SettingCreate(Settings.comOrganizationId);
            await SettingCreate(Settings.awsAccessKeyId);
            await SettingCreate(Settings.awsSecretAccessKey);
            await SettingCreate(Settings.awsEndPoint);
            await SettingCreate(Settings.unitLicenseNumber);
            await SettingCreate(Settings.httpServerAddress);
            await SettingCreate(Settings.maxParallelism);
            await SettingCreate(Settings.numberOfWorkers);
            await SettingCreate(Settings.comSpeechToText);
            await SettingCreate(Settings.swiftEnabled);
            await SettingCreate(Settings.swiftUserName);
            await SettingCreate(Settings.swiftPassword);
            await SettingCreate(Settings.swiftEndPoint);
            await SettingCreate(Settings.keystoneEndPoint);
            await SettingCreate(Settings.customerNo);
            await SettingCreate(Settings.s3Enabled);
            await SettingCreate(Settings.s3AccessKeyId);
            await SettingCreate(Settings.s3SecrectAccessKey);
            await SettingCreate(Settings.s3Endpoint);

            return true;
        }

        /// <summary>
        /// Creates setting with specific name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public async Task<bool> SettingCreate(Settings name)
        {
            using (var db = GetContext())
            {
                //key point
                #region Id = settings.name
                int Id = -1;
                string defaultValue = "default";
                switch (name)
                {
                    case Settings.firstRunDone: Id = 1; defaultValue = "false"; break;
                    case Settings.logLevel: Id = 2; defaultValue = "4"; break;
                    case Settings.logLimit: Id = 3; defaultValue = "25000"; break;
                    case Settings.knownSitesDone: Id = 4; defaultValue = "false"; break;
                    case Settings.fileLocationPicture:
                        if (isLinux)
                        {
                            Id = 5;
                            defaultValue = "/var/www/microting/eform-angular-frontend/eFormAPI/eFormAPI.Web/out/output/datafolder/picture/";
                        }
                        else
                        {
                            Id = 5; 
                            defaultValue = @"output\dataFolder\picture\";
                        } 
                        break;
                    case Settings.fileLocationPdf:
                        if (isLinux)
                        {                            
                            Id = 6;
                            defaultValue = "/var/www/microting/eform-angular-frontend/eFormAPI/eFormAPI.Web/out/output/datafolder/pdf/";
                        }
                        else
                        {
                            Id = 6; 
                            defaultValue = @"output\dataFolder\pdf\";
                        } 
                        break;
                    case Settings.fileLocationJasper:
                        if (isLinux)
                        {
                            Id = 7;
                            defaultValue = "/var/www/microting/eform-angular-frontend/eFormAPI/eFormAPI.Web/out/output/datafolder/reports/";
                        }
                        else
                        {
                            Id = 7; 
                            defaultValue = @"output\dataFolder\reports\";
                        } 
                        break;
                    case Settings.token: Id = 8; defaultValue = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"; break;
                    case Settings.comAddressBasic: Id = 9; defaultValue = "https://basic.microting.com"; break;
                    case Settings.comAddressApi: Id = 10; defaultValue = "https://xxxxxx.xxxxxx.com"; break;
                    case Settings.comAddressPdfUpload: Id = 11; defaultValue = "https://xxxxxx.xxxxxx.com"; break;
                    case Settings.comOrganizationId: Id = 12; defaultValue = "0"; break;
                    case Settings.awsAccessKeyId: Id = 13; defaultValue = "XXX"; break;
                    case Settings.awsSecretAccessKey: Id = 14; defaultValue = "XXX"; break;
                    case Settings.awsEndPoint: Id = 15; defaultValue = "XXX"; break;
                    case Settings.unitLicenseNumber: Id = 16; defaultValue = "0"; break;
                    case Settings.httpServerAddress: Id = 17; defaultValue = "http://localhost:3000"; break;
                    case Settings.maxParallelism: Id = 18; defaultValue = "1"; break;
                    case Settings.numberOfWorkers: Id = 19; defaultValue = "1"; break;
                    case Settings.comSpeechToText: Id = 20; defaultValue = "https://xxxxxx.xxxxxx.com"; break;
                    case Settings.swiftEnabled: Id = 21; defaultValue = "false"; break;
                    case Settings.swiftUserName: Id = 22; defaultValue = "eformsdk"; break;
                    case Settings.swiftPassword: Id = 23; defaultValue = "eformsdktosecretpassword"; break;
                    case Settings.swiftEndPoint: Id = 24; defaultValue = "http://172.16.4.4:8080/swift/v1"; break;
                    case Settings.keystoneEndPoint: Id = 25; defaultValue = "http://172.16.4.4:5000/v2.0"; break;
                    case Settings.customerNo: Id = 26; defaultValue = "0"; break;
                    case Settings.s3Enabled: Id = 27; defaultValue = "false"; break;
                    case Settings.s3AccessKeyId: Id = 28; defaultValue = "XXX"; break;
                    case Settings.s3SecrectAccessKey: Id = 29; defaultValue = "XXX"; break;
                    case Settings.s3Endpoint: Id = 30; defaultValue = "XXX"; break;

                    default:
                        throw new IndexOutOfRangeException(name.ToString() + " is not a known/mapped Settings type");
                }
                #endregion

                settings matchId = await db.settings.SingleOrDefaultAsync(x => x.Id == Id);
                settings matchName = await db.settings.SingleOrDefaultAsync(x => x.Name == name.ToString());

                if (matchName == null)
                {
                    if (matchId != null)
                    {
                        #region there is already a setting with that Id but different name
                        //the old setting data is copied, and new is added
                        settings newSettingBasedOnOld = new settings();
                        newSettingBasedOnOld.Id = (db.settings.Select(x => (int?)x.Id).Max() ?? 0) + 1;
                        newSettingBasedOnOld.Name = matchId.Name.ToString();
                        newSettingBasedOnOld.Value = matchId.Value;

                        db.settings.Add(newSettingBasedOnOld);

                        matchId.Name = name.ToString();
                        matchId.Value = defaultValue;

                        db.SaveChanges();
                        #endregion
                    }
                    else
                    {
                        //its a new setting
                        settings newSetting = new settings();
                        newSetting.Id = Id;
                        newSetting.Name = name.ToString();
                        newSetting.Value = defaultValue;

                        db.settings.Add(newSetting);
                    }
                    db.SaveChanges();
                }
                else
                    if (string.IsNullOrEmpty(matchName.Value))
                    matchName.Value = defaultValue;
            }

            return true;
        }

        /// <summary>
        /// Reads a specific setting from DB
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> SettingRead(Settings name)
        {
            try
            {
                using (var db = GetContext())
                {
                    settings match = await db.settings.SingleAsync(x => x.Name == name.ToString());

                    if (match.Value == null)
                        return "";

                    return match.Value;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(t.GetMethodName("SQLController") + " failed", ex);
            }
        }

        /// <summary>
        /// Updates specific setting with new value in DB
        /// </summary>
        /// <param name="name"></param>
        /// <param name="newValue"></param>
        /// <exception cref="Exception"></exception>
        public async Task SettingUpdate(Settings name, string newValue)
        {
            try
            {
                using (var db = GetContext())
                {
                    settings match = await db.settings.SingleOrDefaultAsync(x => x.Name == name.ToString());

                    if (match == null)
                    {
                        await SettingCreate(name);
                        match = await db.settings.SingleAsync(x => x.Name == name.ToString());
                    }

                    match.Value = newValue;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(t.GetMethodName("SQLController") + " failed", ex);
            }
        }

        //TODO
        public async Task<List<string>> SettingCheckAll()
        {
            List<string> result = new List<string>();
            try
            {
                using (var db = GetContext())
                {
                    if (await db.field_types.CountAsync() == 0)
                    {
                        #region prime FieldTypes
                        //UnitTest_TruncateTable(typeof(field_types).Name);

                        await FieldTypeAdd(1, Constants.Constants.FieldTypes.Text, "Simple text field");
                        await FieldTypeAdd(2, Constants.Constants.FieldTypes.Number, "Simple number field");
                        await FieldTypeAdd(3, Constants.Constants.FieldTypes.None, "Simple text to be displayed");
                        await FieldTypeAdd(4, Constants.Constants.FieldTypes.CheckBox, "Simple check box field");
                        await FieldTypeAdd(5, Constants.Constants.FieldTypes.Picture, "Simple picture field");
                        await FieldTypeAdd(6, Constants.Constants.FieldTypes.Audio, "Simple audio field");
                        await FieldTypeAdd(7, Constants.Constants.FieldTypes.Movie, "Simple movie field");
                        await FieldTypeAdd(8, Constants.Constants.FieldTypes.SingleSelect, "Single selection list");
                        await FieldTypeAdd(9, Constants.Constants.FieldTypes.Comment, "Simple comment field");
                        await FieldTypeAdd(10, Constants.Constants.FieldTypes.MultiSelect, "Simple multi select list");
                        await FieldTypeAdd(11, Constants.Constants.FieldTypes.Date, "Date selection");
                        await FieldTypeAdd(12, Constants.Constants.FieldTypes.Signature, "Simple signature field");
                        await FieldTypeAdd(13, Constants.Constants.FieldTypes.Timer, "Simple timer field");
                        await FieldTypeAdd(14, Constants.Constants.FieldTypes.EntitySearch, "Autofilled searchable items field");
                        await FieldTypeAdd(15, Constants.Constants.FieldTypes.EntitySelect, "Autofilled single selection list");
                        await FieldTypeAdd(16, Constants.Constants.FieldTypes.ShowPdf, "Show PDF");
                        await FieldTypeAdd(17, Constants.Constants.FieldTypes.FieldGroup, "Field group");
                        await FieldTypeAdd(18, Constants.Constants.FieldTypes.SaveButton, "Save eForm");
                        await FieldTypeAdd(19, Constants.Constants.FieldTypes.NumberStepper, "Number stepper field");
                        #endregion
                    }

                    int countVal = db.settings.Count(x => x.Value == "");
                    int countSet = db.settings.Count();

                    if (countSet == 0)
                    {
                        result.Add("NO SETTINGS PRESENT, NEEDS PRIMING!");
                        return result;
                    }

                    foreach (var setting in Enum.GetValues(typeof(Settings)))
                    {
                        try
                        {
                            string readSetting = await SettingRead((Settings)setting);
                            if (string.IsNullOrEmpty(readSetting))
                                result.Add(setting.ToString() + " has an empty value!");
                        }
                        catch
                        {
                            result.Add("There is no setting for " + setting + "! You need to add one");
                        }
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(t.GetMethodName("SQLController") + " failed", ex);
            }
        }
        #endregion

        #region public write log
        
        /// <summary>
        /// Returns a log from corebase
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Log> StartLog(CoreBase core)
        {
            try
            {
                string logLevel = await SettingRead(Settings.logLevel);
                int logLevelInt = int.Parse(logLevel);
                if (log == null)
                    log = new Log(core, this, logLevelInt);
                return log;
            }
            catch (Exception ex)
            {
                throw new Exception(t.GetMethodName("SQLController") + " failed", ex);
            }
        }

        //TODO
        public override async Task<string> WriteLogEntry(LogEntry logEntry)
        {
//            lock (_lockWrite)
//            {
            WriteDebugConsoleLogEntry(logEntry);
                    
            if (logEntry.Level < 0)
                await WriteLogExceptionEntry(logEntry);
            
            if (!isLinux)
            {
                try
                {
                    using (var db = GetContext())
                    {
                        logs newLog = new logs();
                        newLog.CreatedAt = logEntry.Time;
                        newLog.Level = logEntry.Level;
                        newLog.Message = logEntry.Message;
                        newLog.Type = logEntry.Type;

                        db.logs.Add(newLog);
                        await db.SaveChangesAsync();
                        

                        #region clean up of log table
                        int limit = t.Int(logLimit);
                        if (limit > 0)
                        {
                            List<logs> killList = db.logs.Where(x => x.Id <= newLog.Id - limit).ToList();

                            if (killList.Count > 0)
                            {
                                db.logs.RemoveRange(killList);
                                await db.SaveChangesAsync();
                            }
                        }
                        #endregion
                    }
                    return "";
                }
                catch (Exception ex)
                {
                    return t.PrintException(t.GetMethodName("SQLController") + " failed", ex);
                }
                
            }

            return "";
        }

        private void WriteDebugConsoleLogEntry(LogEntry logEntry)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"[DBG] {logEntry.Type}: {logEntry.Message}");
            Console.ForegroundColor = oldColor;
        }

        private void WriteErrorConsoleLogEntry(LogEntry logEntry)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERR] {logEntry.Type}: {logEntry.Message}");
            Console.ForegroundColor = oldColor;
        }

        
        //TODO
        private async Task<string> WriteLogExceptionEntry(LogEntry logEntry)
        {
            try
            {
                using (var db = GetContext())
                {
                    log_exceptions newLog = new log_exceptions();
                    newLog.CreatedAt = logEntry.Time;
                    newLog.Level = logEntry.Level;
                    newLog.Message = logEntry.Message;
                    newLog.Type = logEntry.Type;

                    db.log_exceptions.Add(newLog);
                    await db.SaveChangesAsync();

                    WriteErrorConsoleLogEntry(logEntry);

                    #region clean up of log exception table
                    int limit = t.Int(logLimit);
                    if (limit > 0)
                    {
                        List<log_exceptions> killList = db.log_exceptions.Where(x => x.Id <= newLog.Id - limit).ToList();

                        if (killList.Count > 0)
                        {
                            db.log_exceptions.RemoveRange(killList);
                            await db.SaveChangesAsync();
                        }
                    }
                    #endregion
                }
                return "";
            }
            catch (Exception ex)
            {
                return t.PrintException(t.GetMethodName("SQLController") + " failed", ex);
            }
        }

        /// <summary>
        /// Writes into the log if it failed to write.
        /// </summary>
        /// <param name="logEntries"></param>
        public override void WriteIfFailed(string logEntries)
        {
//            lock (_lockWrite)
//            {
                try
                {
                    File.AppendAllText(@"expection.txt",
                        DateTime.Now.ToString() + " // " + "L:" + "-22" + " // " + "Write logic failed" + " // " + Environment.NewLine
                        + logEntries + Environment.NewLine);
                }
                catch
                {
                    //magic
                }
//            }
        }
        #endregion
        #endregion

        #region private
        #region EformCreateDb
        
        //TODO
        private async Task<int> EformCreateDb(MainElement mainElement)
        {
            try
            {
                using (var db = GetContext())
                {
                    GetConverter();

                    #region mainElement
                    check_lists cl = new check_lists();
                    cl.CreatedAt = DateTime.Now;
                    cl.UpdatedAt = DateTime.Now;
                    cl.Label = mainElement.Label;
                    cl.WorkflowState = Constants.Constants.WorkflowStates.Created;
                    cl.ParentId = null; //MainElements never have parents ;)
                    cl.Repeated = mainElement.Repeated;
                    cl.QuickSyncEnabled = t.Bool(mainElement.EnableQuickSync);
                    cl.Version = 1;
                    cl.CaseType = mainElement.CaseType;
                    cl.FolderName = mainElement.CheckListFolderName;
                    cl.DisplayIndex = mainElement.DisplayOrder;
                    cl.ReviewEnabled = 0; //used for non-MainElements
                    cl.ManualSync = t.Bool(mainElement.ManualSync);
                    cl.ExtraFieldsEnabled = 0; //used for non-MainElements
                    cl.DoneButtonEnabled = 0; //used for non-MainElements
                    cl.ApprovalEnabled = 0; //used for non-MainElements
                    cl.MultiApproval = t.Bool(mainElement.MultiApproval);
                    cl.FastNavigation = t.Bool(mainElement.FastNavigation);
                    cl.DownloadEntities = t.Bool(mainElement.DownloadEntities);
                    cl.OriginalId = mainElement.Id.ToString();

                    await cl.Create(db);

                    int mainId = cl.Id;
                    mainElement.Id = mainId;
                    #endregion

                    await CreateElementList(mainId, mainElement.ElementList);

                    return mainId;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("EformCreateDb failed", ex);
            }
        }

        //TODO
        private async Task CreateElementList(int parentId, List<Element> lstElement)
        {
            foreach (Element element in lstElement)
            {
                if (element.GetType() == typeof(DataElement))
                {
                    DataElement dataE = (DataElement)element;

                    await CreateDataElement(parentId, dataE);
                }

                if (element.GetType() == typeof(GroupElement))
                {
                    GroupElement groupE = (GroupElement)element;

                    await CreateGroupElement(parentId, groupE);
                }
            }
        }

        //TODO
        private async Task CreateGroupElement(int parentId, GroupElement groupElement)
        {
            try
            {
                using (var db = GetContext())
                {
                    check_lists cl = new check_lists();
                    cl.CreatedAt = DateTime.Now;
                    cl.UpdatedAt = DateTime.Now;
                    cl.Label = groupElement.Label;
                    if (groupElement.Description != null)
                        cl.Description = groupElement.Description.InderValue;
                    else
                        cl.Description = "";
                    cl.WorkflowState = Constants.Constants.WorkflowStates.Created;
                    cl.ParentId = parentId;
                    cl.Version = 1;
                    cl.DisplayIndex = groupElement.DisplayOrder;
                    cl.ReviewEnabled = t.Bool(groupElement.ReviewEnabled);
                    cl.ExtraFieldsEnabled = t.Bool(groupElement.ExtraFieldsEnabled);
                    cl.DoneButtonEnabled = t.Bool(groupElement.DoneButtonEnabled);
                    cl.ApprovalEnabled = t.Bool(groupElement.ApprovalEnabled);
                    await cl.Create(db);

                    await CreateElementList(cl.Id, groupElement.ElementList);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CreateGroupElement failed", ex);
            }
        }

        //TODO
        private async Task CreateDataElement(int parentId, DataElement dataElement)
        {
            try
            {
                using (var db = GetContext())
                {
                    check_lists cl = new check_lists();
                    cl.CreatedAt = DateTime.Now;
                    cl.UpdatedAt = DateTime.Now;
                    cl.Label = dataElement.Label;
                    if (dataElement.Description != null)
                        cl.Description = dataElement.Description.InderValue;
                    else
                        cl.Description = "";

                    cl.WorkflowState = Constants.Constants.WorkflowStates.Created;
                    cl.ParentId = parentId;
                    cl.Version = 1;
                    cl.DisplayIndex = dataElement.DisplayOrder;
                    cl.ReviewEnabled = t.Bool(dataElement.ReviewEnabled);
                    cl.ExtraFieldsEnabled = t.Bool(dataElement.ExtraFieldsEnabled);
                    cl.DoneButtonEnabled = t.Bool(dataElement.DoneButtonEnabled);
                    cl.ApprovalEnabled = t.Bool(dataElement.ApprovalEnabled);
                    cl.OriginalId = dataElement.Id.ToString();
                    await cl.Create(db);

                    if (dataElement.DataItemList != null)
                    {
                        foreach (DataItem dataItem in dataElement.DataItemList)
                        {
                            await CreateDataItem(null, cl.Id, dataItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CreateDataElement failed", ex);
            }
        }

        
        //TODO
//        private void CreateDataItemGroup(int elementId, FieldContainer fieldGroup)
//        {
//            try
//            {
//                using (var db = GetContext())
//                {
//                    string typeStr = fieldGroup.GetType().ToString().Remove(0, 10); //10 = "eFormData.".Length
//                    int fieldTypeId = Find(typeStr);
//
//                    fields field = new fields();
//                    field.ParentFieldId = null;
//                    field.Color = fieldGroup.Color;
//                    //CDataValue description = new CDataValue();
//                    //description.InderValue = fieldGroup.Description;
//                    field.Description = fieldGroup.Description.InderValue;
//                    field.DisplayIndex = fieldGroup.DisplayOrder;
//                    field.Label = fieldGroup.Label;
//
//                    field.CreatedAt = DateTime.Now;
//                    field.UpdatedAt = DateTime.Now;
//                    field.WorkflowState = Constants.Constants.WorkflowStates.Created;
//                    field.CheckListId = elementId;
//                    field.FieldTypeId = fieldTypeId;
//                    field.Version = 1;
//
//                    field.DefaultValue = fieldGroup.Value;
//
//                    db.fields.Add(field);
//                    db.SaveChanges();
//
//                    db.field_versions.Add(MapFieldVersions(field));
//                    db.SaveChanges();
//
//                    if (fieldGroup.DataItemList != null)
//                    {
//                        foreach (DataItem dataItem in fieldGroup.DataItemList)
//                        {
//                            CreateDataItem(field.Id, elementId, dataItem);
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("CreateDataItemGroup failed", ex);
//            }
//        }

        
        //TODO
        private async Task CreateDataItem(int? parentFieldId, int elementId, DataItem dataItem)
        {
            try
            {
                using (var db = GetContext())
                {
                    string typeStr = dataItem.GetType().Name;

                    /*
                     * Hack for making the FieldContainer work, since it's actually a FieldGroup 
                     */
                    if (typeStr.Equals("FieldContainer"))
                        typeStr = "FieldGroup";

                    int fieldTypeId = Find(typeStr);

                    fields field = new fields();
                    field.Color = dataItem.Color;
                    field.ParentFieldId = parentFieldId;
                    if (dataItem.Description != null)
                        field.Description = dataItem.Description.InderValue;
                    else
                        field.Description = "";
                    field.DisplayIndex = dataItem.DisplayOrder;
                    field.Label = dataItem.Label;
                    field.Mandatory = t.Bool(dataItem.Mandatory);
                    field.ReadOnly = t.Bool(dataItem.ReadOnly);
                    field.Dummy = t.Bool(dataItem.Dummy);

                    field.CreatedAt = DateTime.Now;
                    field.UpdatedAt = DateTime.Now;
                    field.WorkflowState = Constants.Constants.WorkflowStates.Created;
                    field.CheckListId = elementId;
                    field.FieldTypeId = fieldTypeId;
                    field.Version = 1;
                    field.OriginalId = dataItem.Id.ToString();

                    bool isSaved = false; // This is done, because we need to have the current field Id, for giving it onto the child fields in a FieldGroup

                    #region dataItem type
                    //KEY POINT - mapping
                    switch (typeStr)
                    {
                        case Constants.Constants.FieldTypes.Audio:
                            Audio audio = (Audio)dataItem;
                            field.Multi = audio.Multi;
                            break;

                        case Constants.Constants.FieldTypes.CheckBox:
                            CheckBox checkBox = (CheckBox)dataItem;
                            field.DefaultValue = checkBox.DefaultValue.ToString();
                            field.Selected = t.Bool(checkBox.Selected);
                            break;

                        case Constants.Constants.FieldTypes.Comment:
                            Comment comment = (Comment)dataItem;
                            field.DefaultValue = comment.Value;
                            field.MaxLength = comment.Maxlength;
                            field.SplitScreen = t.Bool(comment.SplitScreen);
                            break;

                        case Constants.Constants.FieldTypes.Date:
                            Date date = (Date)dataItem;
                            field.DefaultValue = date.DefaultValue;
                            field.MinValue = date.MinValue.ToString("yyyy-MM-dd");
                            field.MaxValue = date.MaxValue.ToString("yyyy-MM-dd");
                            break;

                        case Constants.Constants.FieldTypes.None:
                            break;

                        case Constants.Constants.FieldTypes.Number:
                            Number number = (Number)dataItem;
                            field.MinValue = number.MinValue.ToString();
                            field.MaxValue = number.MaxValue.ToString();
                            field.DefaultValue = number.DefaultValue.ToString();
                            field.DecimalCount = number.DecimalCount;
                            field.UnitName = number.UnitName;
                            break;

                        case Constants.Constants.FieldTypes.NumberStepper:
                            NumberStepper numberStepper = (NumberStepper)dataItem;
                            field.MinValue = numberStepper.MinValue.ToString();
                            field.MaxValue = numberStepper.MaxValue.ToString();
                            field.DefaultValue = numberStepper.DefaultValue.ToString();
                            field.DecimalCount = numberStepper.DecimalCount;
                            field.UnitName = numberStepper.UnitName;
                            break;

                        case Constants.Constants.FieldTypes.MultiSelect:
                            MultiSelect multiSelect = (MultiSelect)dataItem;
                            field.KeyValuePairList = PairBuild(multiSelect.KeyValuePairList);
                            break;

                        case Constants.Constants.FieldTypes.Picture:
                            Picture picture = (Picture)dataItem;
                            field.Multi = picture.Multi;
                            field.GeolocationEnabled = t.Bool(picture.GeolocationEnabled);
                            break;

                        case Constants.Constants.FieldTypes.SaveButton:
                            SaveButton saveButton = (SaveButton)dataItem;
                            field.DefaultValue = saveButton.Value;
                            break;

                        case Constants.Constants.FieldTypes.ShowPdf:
                            ShowPdf showPdf = (ShowPdf)dataItem;
                            field.DefaultValue = showPdf.Value;
                            break;

                        case Constants.Constants.FieldTypes.Signature:
                            break;

                        case Constants.Constants.FieldTypes.SingleSelect:
                            SingleSelect singleSelect = (SingleSelect)dataItem;
                            field.KeyValuePairList = PairBuild(singleSelect.KeyValuePairList);
                            break;

                        case Constants.Constants.FieldTypes.Text:
                            Text text = (Text)dataItem;
                            field.DefaultValue = text.Value;
                            field.MaxLength = text.MaxLength;
                            field.GeolocationEnabled = t.Bool(text.GeolocationEnabled);
                            field.GeolocationForced = t.Bool(text.GeolocationForced);
                            field.GeolocationHidden = t.Bool(text.GeolocationHidden);
                            field.BarcodeEnabled = t.Bool(text.BarcodeEnabled);
                            field.BarcodeType = text.BarcodeType;
                            break;

                        case Constants.Constants.FieldTypes.Timer:
                            Timer timer = (Timer)dataItem;
                            field.SplitScreen = t.Bool(timer.StopOnSave);
                            break;

                        //-------

                        case Constants.Constants.FieldTypes.EntitySearch:
                            EntitySearch entitySearch = (EntitySearch)dataItem;
                            field.EntityGroupId = entitySearch.EntityTypeId;
                            field.DefaultValue = entitySearch.DefaultValue.ToString();
                            field.IsNum = t.Bool(entitySearch.IsNum);
                            field.QueryType = entitySearch.QueryType;
                            field.MinValue = entitySearch.MinSearchLenght.ToString();
                            break;

                        case Constants.Constants.FieldTypes.EntitySelect:
                            EntitySelect entitySelect = (EntitySelect)dataItem;
                            field.EntityGroupId = entitySelect.Source;
                            field.DefaultValue = entitySelect.DefaultValue.ToString();
                            break;

                        case Constants.Constants.FieldTypes.FieldGroup:
                            FieldContainer fg = (FieldContainer)dataItem;
                            field.DefaultValue = fg.Value;
                            await field.Create(db);
                            
                            isSaved = true;
                            if (fg.DataItemList != null)
                            {
                                foreach (DataItem data_item in fg.DataItemList)
                                {
                                    await CreateDataItem(field.Id, elementId, data_item);
                                }
                            }
                            break;

                        default:
                            throw new IndexOutOfRangeException(dataItem.GetType().ToString() + " is not a known/mapped DataItem type");
                    }
                    #endregion
                    if (!isSaved)
                    {
                        await field.Create(db);
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception("CreateDataItem failed", ex);
            }
        }
        #endregion

        #region EformReadDb
        
        /// <summary>
        /// Gets element with specifik id from DB
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<Element> GetElement(int elementId)
        {
            try
            {
                using (var db = GetContext())
                {
                    Element element;

                    //getting element's possible element children
                    List<check_lists> lstElement = db.check_lists.Where(x => x.ParentId == elementId).ToList();


                    if (lstElement.Count > 0) //GroupElement
                    {
                        //list for the DataItems
                        List<Element> lst = new List<Element>();

                        //the actual DataElement
                        try
                        {
                            check_lists cl = await db.check_lists.SingleAsync(x => x.Id == elementId);
                            GroupElement gElement = new GroupElement(cl.Id, 
                                cl.Label, 
                                t.Int(cl.DisplayIndex), 
                                cl.Description, 
                                t.Bool(cl.ApprovalEnabled), 
                                t.Bool(cl.ReviewEnabled),
                                t.Bool(cl.DoneButtonEnabled), 
                                t.Bool(cl.ExtraFieldsEnabled), 
                                "", 
                                t.Bool(cl.QuickSyncEnabled), 
                                lst);

                            //the actual Elements
                            foreach (var subElement in lstElement)
                            {
                                lst.Add(await GetElement(subElement.Id));
                            }
                            element = gElement;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Failed to find check_list with Id:" + elementId, ex);
                        }
                    }
                    else //DataElement
                    {
                        //the actual DataElement
                        try
                        {
                            check_lists cl = await db.check_lists.SingleAsync(x => x.Id == elementId);
                            DataElement dElement = new DataElement(cl.Id, 
                                cl.Label,
                                t.Int(cl.DisplayIndex), 
                                cl.Description, 
                                t.Bool(cl.ApprovalEnabled), 
                                t.Bool(cl.ReviewEnabled),
                                t.Bool(cl.DoneButtonEnabled), 
                                t.Bool(cl.ExtraFieldsEnabled), 
                                "", 
                                t.Bool(cl.QuickSyncEnabled), 
                                new List<DataItemGroup>(), 
                                new List<DataItem>());

                            //the actual DataItems
                            List<fields> lstFields = db.fields.Where(x => x.CheckListId == elementId && x.ParentFieldId == null).ToList();
                            foreach (var field in lstFields)
                            {
                                await GetDataItem(dElement.DataItemList, dElement.DataItemGroupList, field);
                            }
                            element = dElement;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Failed to find check_list with Id:" + elementId, ex);
                        }
                    }
                    return element;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("GetElement failed", ex);
            }
        }

        /// <summary>
        /// Gets data item with specific id from DB
        /// </summary>
        /// <param name="lstDataItem"></param>
        /// <param name="lstDataItemGroup"></param>
        /// <param name="dataItemId"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <exception cref="Exception"></exception>
        private async Task GetDataItem(List<DataItem> lstDataItem, List<DataItemGroup> lstDataItemGroup, fields field)
        {
            try
            {
                using (var db = GetContext())
                {
//                    fields field = db.fields.SingleAsync(x => x.Id == dataItemId);
                    string fieldTypeStr = Find(t.Int(field.FieldTypeId));

                    //KEY POINT - mapping
                    switch (fieldTypeStr)
                    {
                        case Constants.Constants.FieldTypes.Audio:
                            lstDataItem.Add(new Audio(t.Int(field.Id), t.Bool(field.Mandatory), t.Bool(field.ReadOnly), field.Label, field.Description, field.Color, t.Int(field.DisplayIndex), t.Bool(field.Dummy),
                                t.Int(field.Multi)));
                            break;

                        case Constants.Constants.FieldTypes.CheckBox:
                            lstDataItem.Add(new CheckBox(t.Int(field.Id), t.Bool(field.Mandatory), t.Bool(field.ReadOnly), field.Label, field.Description, field.Color, t.Int(field.DisplayIndex), t.Bool(field.Dummy),
                                t.Bool(field.DefaultValue), t.Bool(field.Selected)));
                            break;

                        case Constants.Constants.FieldTypes.Comment:
                            lstDataItem.Add(new Comment(t.Int(field.Id), t.Bool(field.Mandatory), t.Bool(field.ReadOnly), field.Label, field.Description, field.Color, t.Int(field.DisplayIndex), t.Bool(field.Dummy),
                                field.DefaultValue, t.Int(field.MaxLength), t.Bool(field.SplitScreen)));
                            break;

                        case Constants.Constants.FieldTypes.Date:
                            lstDataItem.Add(new Date(t.Int(field.Id), t.Bool(field.Mandatory), t.Bool(field.ReadOnly), field.Label, field.Description, field.Color, t.Int(field.DisplayIndex), t.Bool(field.Dummy),
                                DateTime.Parse(field.MinValue), DateTime.Parse(field.MaxValue), field.DefaultValue));
                            break;

                        case Constants.Constants.FieldTypes.None:
                            lstDataItem.Add(new None(t.Int(field.Id), t.Bool(field.Mandatory), t.Bool(field.ReadOnly), field.Label, field.Description, field.Color, t.Int(field.DisplayIndex), t.Bool(field.Dummy)));
                            break;

                        case Constants.Constants.FieldTypes.Number:
                            lstDataItem.Add(new Number(t.Int(field.Id), t.Bool(field.Mandatory), t.Bool(field.ReadOnly), field.Label, field.Description, field.Color, t.Int(field.DisplayIndex), t.Bool(field.Dummy),
                                field.MinValue, field.MaxValue, int.Parse(field.DefaultValue), t.Int(field.DecimalCount), field.UnitName));
                            break;

                        case Constants.Constants.FieldTypes.NumberStepper:
                            lstDataItem.Add(new NumberStepper(t.Int(field.Id), t.Bool(field.Mandatory), t.Bool(field.ReadOnly), field.Label, field.Description, field.Color, t.Int(field.DisplayIndex), t.Bool(field.Dummy),
                                field.MinValue, field.MaxValue, int.Parse(field.DefaultValue), t.Int(field.DecimalCount), field.UnitName));
                            break;

                        case Constants.Constants.FieldTypes.MultiSelect:
                            lstDataItem.Add(new MultiSelect(t.Int(field.Id), t.Bool(field.Mandatory), t.Bool(field.ReadOnly), field.Label, field.Description, field.Color, t.Int(field.DisplayIndex), t.Bool(field.Dummy),
                                PairRead(field.KeyValuePairList)));
                            break;

                        case Constants.Constants.FieldTypes.Picture:
                            lstDataItem.Add(new Picture(t.Int(field.Id), t.Bool(field.Mandatory), t.Bool(field.ReadOnly), field.Label, field.Description, field.Color, t.Int(field.DisplayIndex), t.Bool(field.Dummy),
                                t.Int(field.Multi), t.Bool(field.GeolocationEnabled)));
                            break;

                        case Constants.Constants.FieldTypes.SaveButton:
                            lstDataItem.Add(new SaveButton(t.Int(field.Id), t.Bool(field.Mandatory), t.Bool(field.ReadOnly), field.Label, field.Description, field.Color, t.Int(field.DisplayIndex), t.Bool(field.Dummy),
                                field.DefaultValue));
                            break;

                        case Constants.Constants.FieldTypes.ShowPdf:
                            lstDataItem.Add(new ShowPdf(t.Int(field.Id), t.Bool(field.Mandatory), t.Bool(field.ReadOnly), field.Label, field.Description, field.Color, t.Int(field.DisplayIndex), t.Bool(field.Dummy),
                                field.DefaultValue));
                            break;

                        case Constants.Constants.FieldTypes.Signature:
                            lstDataItem.Add(new Signature(t.Int(field.Id), t.Bool(field.Mandatory), t.Bool(field.ReadOnly), field.Label, field.Description, field.Color, t.Int(field.DisplayIndex), t.Bool(field.Dummy)));
                            break;

                        case Constants.Constants.FieldTypes.SingleSelect:
                            lstDataItem.Add(new SingleSelect(t.Int(field.Id), t.Bool(field.Mandatory), t.Bool(field.ReadOnly), field.Label, field.Description, field.Color, t.Int(field.DisplayIndex), t.Bool(field.Dummy),
                                PairRead(field.KeyValuePairList)));
                            break;

                        case Constants.Constants.FieldTypes.Text:
                            lstDataItem.Add(new Text(t.Int(field.Id), t.Bool(field.Mandatory), t.Bool(field.ReadOnly), field.Label, field.Description, field.Color, t.Int(field.DisplayIndex), t.Bool(field.Dummy),
                                field.DefaultValue, t.Int(field.MaxLength), t.Bool(field.GeolocationEnabled), t.Bool(field.GeolocationForced), t.Bool(field.GeolocationHidden), t.Bool(field.BarcodeEnabled), field.BarcodeType));
                            break;

                        case Constants.Constants.FieldTypes.Timer:
                            lstDataItem.Add(new Timer(t.Int(field.Id), t.Bool(field.Mandatory), t.Bool(field.ReadOnly), field.Label, field.Description, field.Color, t.Int(field.DisplayIndex), t.Bool(field.Dummy),
                                t.Bool(field.StopOnSave)));
                            break;

                        case Constants.Constants.FieldTypes.EntitySearch:
                            lstDataItem.Add(new EntitySearch(t.Int(field.Id), t.Bool(field.Mandatory), t.Bool(field.ReadOnly), field.Label, field.Description, field.Color, t.Int(field.DisplayIndex), t.Bool(field.Dummy),
                                t.Int(field.DefaultValue), t.Int(field.EntityGroupId), t.Bool(field.IsNum), field.QueryType, t.Int(field.MinValue), t.Bool(field.BarcodeEnabled), field.BarcodeType));
                            break;

                        case Constants.Constants.FieldTypes.EntitySelect:
                            lstDataItem.Add(new EntitySelect(t.Int(field.Id), t.Bool(field.Mandatory), t.Bool(field.ReadOnly), field.Label, field.Description, field.Color, t.Int(field.DisplayIndex), t.Bool(field.Dummy),
                                t.Int(field.DefaultValue), t.Int(field.EntityGroupId)));
                            break;

                        case Constants.Constants.FieldTypes.FieldGroup:
                            List<DataItem> lst = new List<DataItem>();
                            //CDataValue description = new CDataValue();
                            //description.InderValue = f.description;
                            lstDataItemGroup.Add(new FieldGroup(field.Id.ToString(), field.Label, field.Description, field.Color, t.Int(field.DisplayIndex), field.DefaultValue, lst));
                            //lstDataItemGroup.Add(new DataItemGroup(f.Id.ToString(), f.label, f.description, f.color, t.Int(f.display_index), f.default_value, lst));

                            //the actual DataItems
                            List<fields> lstFields = db.fields.Where(x => x.ParentFieldId == field.Id).ToList();
                            foreach (var subField in lstFields)
                                await GetDataItem(lst, null, subField); //null, due to FieldGroup, CANT have fieldGroups under them
                            break;

                        default:
                            throw new IndexOutOfRangeException(field.FieldTypeId + " is not a known/mapped DataItem type");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("GetDataItem failed", ex);
            }
        }

        //TODO
        private void GetConverter()
        {
            if (converter == null)
            {
                try
                {
                    using (var db = GetContext())
                    {
                        converter = new List<Holder>();

                        List<field_types> lstFieldType = db.field_types.ToList();

                        foreach (var fieldType in lstFieldType)
                        {
                            converter.Add(new Holder(fieldType.Id, fieldType.FieldType));
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("GetConverter failed", ex);
                }                
            }
        }
        #endregion

        #region tags
        
        /// <summary>
        /// Gets all tags from DB
        /// </summary>
        /// <param name="includeRemoved"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<Tag>> GetAllTags(bool includeRemoved)
        {
            List<Tag> tags = new List<Tag>();
            try
            {
                using (var db = GetContext())
                {
                    List<tags> matches = null;
                    if (!includeRemoved)
                        matches = await db.tags.Where(x => x.WorkflowState == Constants.Constants.WorkflowStates.Created).ToListAsync();
                    else
                        matches = await db.tags.ToListAsync();

                    foreach (tags tag in matches)
                    {
                        Tag t = new Tag(tag.Id, tag.Name, tag.TaggingsCount);
                        tags.Add(t);
                    }
                }
                return tags;
            }
            catch (Exception ex)
            {
                throw new Exception("GetAllTags failed", ex);
            }
        }

        /// <summary>
        /// Create tag with given name and saves it in DB
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> TagCreate(string name)
        {
            try
            {
                using (var db = GetContext())
                {
                    tags tag = await db.tags.SingleOrDefaultAsync(x => x.Name == name);
                    if (tag == null)
                    {
                        tag = new tags();
                        tag.Name = name;
                        await tag.Create(db);
                        return tag.Id;
                    } else
                    {
                        tag.WorkflowState = Constants.Constants.WorkflowStates.Created;
                        await tag.Update(db);
                        return tag.Id;
                    }                    
                }
                //return ;
            }
            catch (Exception ex)
            {
                throw new Exception("TagCreate failed", ex);
            }
        }

        /// <summary>
        /// Deletes tag with specific id from DB
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> TagDelete(int tagId)
        {
            try
            {
                using (var db = GetContext())
                {
                    tags tag = await db.tags.SingleOrDefaultAsync(x => x.Id == tagId);
                    if (tag != null)                    
                    {
                        await tag.Delete(db);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("TagDelete failed", ex);
            }
        }
        #endregion

        #region help methods
        
        /// <summary>
        /// Finds field type by id
        /// </summary>
        /// <param name="fieldTypeId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private string Find(int fieldTypeId)
        {
            foreach (var holder in converter)
            {
                if (holder.Index == fieldTypeId)
                    return holder.FieldType;
            }
            throw new Exception("Find failed. Not known fieldType for fieldTypeId: " + fieldTypeId);
        }

        /// <summary>
        /// Find field type by type string
        /// </summary>
        /// <param name="typeStr"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private int Find(string typeStr)
        {
            foreach (var holder in converter)
            {
                if (holder.FieldType == typeStr)
                    return holder.Index;
            }
            throw new Exception("Find failed. Not known fieldTypeId for typeStr: " + typeStr);
        }

        //TODO
        private string PairBuild(List<KeyValuePair> lst)
        {
            string str = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><hash>";
            string inderStr;
            int index = 1;

            foreach (KeyValuePair kVP in lst)
            {
                inderStr = "<" + index + ">";

                inderStr += "<key>" + kVP.Value + "</key>";
                inderStr += "<selected>" + kVP.Selected + "</selected>";
                inderStr += "<displayIndex>" + kVP.DisplayOrder + "</displayIndex>";

                inderStr += "</" + index + ">";

                str += inderStr;
                index += 1;
            }

            str += "</hash>";
            return str;
        }

        //TODO
        public List<KeyValuePair> PairRead(string str)
        {
            List<KeyValuePair> list = new List<KeyValuePair>();
            str = t.Locate(str, "<hash>", "</hash>");

            bool flag = true;
            int index = 1;
            string keyValue, displayIndex;
            bool selected;

            while (flag)
            {
                string inderStr = t.Locate(str, "<" + index + ">", "</" + index + ">");

                keyValue = t.Locate(inderStr, "<key>", "</");
                selected = bool.Parse(t.Locate(inderStr.ToLower(), "<selected>", "</"));
                displayIndex = t.Locate(inderStr, "<displayIndex>", "</");

                list.Add(new KeyValuePair(index.ToString(), keyValue, selected, displayIndex));

                index += 1;

                if (t.Locate(str, "<" + index + ">", "</" + index + ">") == "")
                    flag = false;
            }

            return list;
        }

        //ToDo
        private string PairMatch(List<KeyValuePair> keyValuePairs, string match)
        {
            foreach (var item in keyValuePairs)
            {
                if (item.Key == match)
                    return item.Value;
            }

            return null;
        }
        #endregion

        
        #endregion       

        /// <summary>
        /// Adding field type to DB 
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="fieldType"></param>
        /// <param name="description"></param>
        private async Task FieldTypeAdd(int Id, string fieldType, string description)
        {
            using (var db = GetContext())
            {
                if (db.field_types.Count(x => x.FieldType == fieldType) == 0)
                {
                    field_types fT = new field_types();
                    fT.FieldType = fieldType;
                    fT.Description = description;
                    await fT.Create(db);
                }                
            }
        }
    }    
}