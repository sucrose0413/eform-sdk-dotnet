﻿using eFormCore;
using eFormCore.Helpers;
using eFormData;
using eFormShared;
using eFormSqlController;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace eFormSDK.Integration.Tests
{
    [TestFixture]
    public class SqlControllerTestWorker : DbTestFixture
    {
        private Core sut;
        private TestHelpers testHelpers;
        private string path;

        public override void DoSetup()
        {
            #region Setup SettingsTableContent

            SqlController sql = new SqlController(ConnectionString);
            sql.SettingUpdate(Settings.token, "abc1234567890abc1234567890abcdef");
            sql.SettingUpdate(Settings.firstRunDone, "true");
            sql.SettingUpdate(Settings.knownSitesDone, "true");
            #endregion

            sut = new Core();
            sut.HandleCaseCreated += EventCaseCreated;
            sut.HandleCaseRetrived += EventCaseRetrived;
            sut.HandleCaseCompleted += EventCaseCompleted;
            sut.HandleCaseDeleted += EventCaseDeleted;
            sut.HandleFileDownloaded += EventFileDownloaded;
            sut.HandleSiteActivated += EventSiteActivated;
            sut.StartSqlOnly(ConnectionString);
            path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            path = System.IO.Path.GetDirectoryName(path).Replace(@"file:\", "");
            sut.SetPicturePath(path + @"\output\dataFolder\picture\");
            sut.SetPdfPath(path + @"\output\dataFolder\pdf\");
            sut.SetJasperPath(path + @"\output\dataFolder\reports\");
            testHelpers = new TestHelpers();
            //sut.StartLog(new CoreBase());
        }


        #region Worker

        [Test]
        public void SQL_Worker_WorkerGetAll_ReturnsAllWorkers()
        {
            // Arrance
            #region Arrance

            #region Checklist

            check_lists Cl1 = CreateTemplate("A1", "D1", "caseType1", "WhereItIs", 1, 0);

            #endregion

            #region SubCheckList

            check_lists Cl2 = CreateSubTemplate("A2", "D2", "caseType2", 2, 0, Cl1);

            #endregion

            #region Fields

            #region field1


            fields f1 = CreateField(1, "barcode", Cl2, "e2f4fb", "custom", null, "", "Comment field description",
                5, 1, DbContext.field_types.Where(x => x.field_type == "picture").First(), 0, 0, 1, 0, "Comment field", 1, 55, "55", "0", 0, 0, null, 1, 0,
                0, 0, "", 49);

            #endregion

            #region field2


            fields f2 = CreateField(1, "barcode", Cl2, "f5eafa", "custom", null, "", "showPDf Description",
                45, 1, DbContext.field_types.Where(x => x.field_type == "comment").First(), 0, 1, 0, 0,
                "ShowPdf", 0, 5, "5", "0", 0, 0, null, 0, 0, 0, 0, "", 9);


            #endregion

            #region field3

            fields f3 = CreateField(0, "barcode", Cl2, "f0f8db", "custom", 3, "", "Number Field Description",
                83, 0, DbContext.field_types.Where(x => x.field_type == "picture").First(), 0, 0, 1, 0,
                "Numberfield", 1, 8, "4865", "0", 0, 1, null, 1, 0, 0, 0, "", 1);


            #endregion

            #region field4


            fields f4 = CreateField(1, "barcode", Cl2, "fff6df", "custom", null, "", "date Description",
                84, 0, DbContext.field_types.Where(x => x.field_type == "picture").First(), 0, 0, 1, 0,
                "Date", 1, 666, "41153", "0", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field5

            fields f5 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                85, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field6

            fields f6 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                86, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field7

            fields f7 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                87, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field8

            fields f8 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                88, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field9

            fields f9 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                89, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field10

            fields f10 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                90, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #endregion

            #region Workers

            #region worker1
            workers worker1 = CreateWorker("aa@tak.dk", "Arne", "Jensen", 21);

            #endregion

            #region worker2
            workers worker2 = CreateWorker("ab@tak.dk", "Lasse", "Johansen", 44);

            #endregion

            #region worker3
            workers worker3 = CreateWorker("ac@tak.dk", "Svend", "Jensen", 22);

            #endregion

            #region worker4
            workers worker4 = CreateWorker("ad@tak.dk", "Bjarne", "Nielsen", 23);

            #endregion

            #region worker5
            workers worker5 = CreateWorker("ae@tak.dk", "Ib", "Hansen", 24);

            #endregion

            #region worker6
            workers worker6 = CreateWorker("af@tak.dk", "Hozan", "Aziz", 25);

            #endregion

            #region worker7
            workers worker7 = CreateWorker("ag@tak.dk", "Nicolai", "Peders", 26);

            #endregion

            #region worker8
            workers worker8 = CreateWorker("ah@tak.dk", "Amin", "Safari", 27);

            #endregion

            #region worker9
            workers worker9 = CreateWorker("ai@tak.dk", "Leo", "Rebaz", 28);

            #endregion

            #region worker10
            workers worker10 = CreateWorker("aj@tak.dk", "Stig", "Berthelsen", 29);

            #endregion

            #endregion

            #region sites

            #region Site1
            sites site1 = CreateSite("SiteName1", 88);

            #endregion

            #region Site2
            sites site2 = CreateSite("SiteName2", 89);

            #endregion

            #region Site3
            sites site3 = CreateSite("SiteName3", 90);

            #endregion

            #region Site4
            sites site4 = CreateSite("SiteName4", 91);

            #endregion

            #region Site5
            sites site5 = CreateSite("SiteName5", 92);

            #endregion

            #region Site6
            sites site6 = CreateSite("SiteName6", 93);

            #endregion

            #region Site7
            sites site7 = CreateSite("SiteName7", 94);

            #endregion

            #region Site8
            sites site8 = CreateSite("SiteName8", 95);

            #endregion

            #region Site9
            sites site9 = CreateSite("SiteName9", 96);

            #endregion

            #region Site10
            sites site10 = CreateSite("SiteName10", 97);

            #endregion

            #endregion

            #region units
            units unit = CreateUnit(48, 49, site1, 348);

            #endregion

            #region site_workers
            site_workers site_workers = CreateSiteWorker(55, site1, worker1);

            #endregion

            #endregion
            // Act

            var getAllCreatedWorkers = sut.WorkerGetAll(Constants.WorkflowStates.Created, 0, 1);
            var getAllRemovedWorkers = sut.WorkerGetAll(Constants.WorkflowStates.Removed, 0, 1);


            // Assert

            Assert.AreEqual(10, getAllCreatedWorkers.Count());
            Assert.AreEqual(0, getAllRemovedWorkers.Count());

            Assert.AreEqual(worker1.first_name, getAllCreatedWorkers[0].FirstName);
            Assert.AreEqual(worker2.first_name, getAllCreatedWorkers[1].FirstName);
            Assert.AreEqual(worker3.first_name, getAllCreatedWorkers[2].FirstName);
            Assert.AreEqual(worker4.first_name, getAllCreatedWorkers[3].FirstName);
            Assert.AreEqual(worker5.first_name, getAllCreatedWorkers[4].FirstName);
            Assert.AreEqual(worker6.first_name, getAllCreatedWorkers[5].FirstName);
            Assert.AreEqual(worker7.first_name, getAllCreatedWorkers[6].FirstName);
            Assert.AreEqual(worker8.first_name, getAllCreatedWorkers[7].FirstName);
            Assert.AreEqual(worker9.first_name, getAllCreatedWorkers[8].FirstName);
            Assert.AreEqual(worker10.first_name, getAllCreatedWorkers[9].FirstName);


            Assert.AreEqual(worker1.last_name, getAllCreatedWorkers[0].LastName);
            Assert.AreEqual(worker2.last_name, getAllCreatedWorkers[1].LastName);
            Assert.AreEqual(worker3.last_name, getAllCreatedWorkers[2].LastName);
            Assert.AreEqual(worker4.last_name, getAllCreatedWorkers[3].LastName);
            Assert.AreEqual(worker5.last_name, getAllCreatedWorkers[4].LastName);
            Assert.AreEqual(worker6.last_name, getAllCreatedWorkers[5].LastName);
            Assert.AreEqual(worker7.last_name, getAllCreatedWorkers[6].LastName);
            Assert.AreEqual(worker8.last_name, getAllCreatedWorkers[7].LastName);
            Assert.AreEqual(worker9.last_name, getAllCreatedWorkers[8].LastName);
            Assert.AreEqual(worker10.last_name, getAllCreatedWorkers[9].LastName);
        }
        [Test]
        public void SQL_Worker_WorkerCreate_ReturnsWorkerId()
        {
            // Arrance

            // Act
            var match = sut.WorkerCreate(55, "Arne", "Jensen", "aa@tak.dk");
            // Assert

            var workers = DbContext.workers.AsNoTracking().ToList();

            Assert.NotNull(match);
            Assert.AreEqual(1, workers.Count());
            Assert.AreEqual(Constants.WorkflowStates.Created, workers[0].workflow_state);
        }
        [Test]
        public void SQL_Worker_WorkerNameRead_ReadsName()
        {
            // Arrance
            #region Arrance

            #region Checklist

            check_lists Cl1 = CreateTemplate("A1", "D1", "caseType1", "WhereItIs", 1, 0);

            #endregion

            #region SubCheckList

            check_lists Cl2 = CreateSubTemplate("A2", "D2", "caseType2", 2, 0, Cl1);

            #endregion

            #region Fields

            #region field1


            fields f1 = CreateField(1, "barcode", Cl2, "e2f4fb", "custom", null, "", "Comment field description",
                5, 1, DbContext.field_types.Where(x => x.field_type == "picture").First(), 0, 0, 1, 0, "Comment field", 1, 55, "55", "0", 0, 0, null, 1, 0,
                0, 0, "", 49);

            #endregion

            #region field2


            fields f2 = CreateField(1, "barcode", Cl2, "f5eafa", "custom", null, "", "showPDf Description",
                45, 1, DbContext.field_types.Where(x => x.field_type == "comment").First(), 0, 1, 0, 0,
                "ShowPdf", 0, 5, "5", "0", 0, 0, null, 0, 0, 0, 0, "", 9);


            #endregion

            #region field3

            fields f3 = CreateField(0, "barcode", Cl2, "f0f8db", "custom", 3, "", "Number Field Description",
                83, 0, DbContext.field_types.Where(x => x.field_type == "picture").First(), 0, 0, 1, 0,
                "Numberfield", 1, 8, "4865", "0", 0, 1, null, 1, 0, 0, 0, "", 1);


            #endregion

            #region field4


            fields f4 = CreateField(1, "barcode", Cl2, "fff6df", "custom", null, "", "date Description",
                84, 0, DbContext.field_types.Where(x => x.field_type == "picture").First(), 0, 0, 1, 0,
                "Date", 1, 666, "41153", "0", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field5

            fields f5 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                85, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field6

            fields f6 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                86, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field7

            fields f7 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                87, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field8

            fields f8 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                88, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field9

            fields f9 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                89, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field10

            fields f10 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                90, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #endregion

            #region Workers

            #region worker1
            workers worker1 = CreateWorker("aa@tak.dk", "Arne", "Jensen", 21);

            #endregion

            #region worker2
            workers worker2 = CreateWorker("ab@tak.dk", "Lasse", "Johansen", 44);

            #endregion

            #region worker3
            workers worker3 = CreateWorker("ac@tak.dk", "Svend", "Jensen", 22);

            #endregion

            #region worker4
            workers worker4 = CreateWorker("ad@tak.dk", "Bjarne", "Nielsen", 23);

            #endregion

            #region worker5
            workers worker5 = CreateWorker("ae@tak.dk", "Ib", "Hansen", 24);

            #endregion

            #region worker6
            workers worker6 = CreateWorker("af@tak.dk", "Hozan", "Aziz", 25);

            #endregion

            #region worker7
            workers worker7 = CreateWorker("ag@tak.dk", "Nicolai", "Peders", 26);

            #endregion

            #region worker8
            workers worker8 = CreateWorker("ah@tak.dk", "Amin", "Safari", 27);

            #endregion

            #region worker9
            workers worker9 = CreateWorker("ai@tak.dk", "Leo", "Rebaz", 28);

            #endregion

            #region worker10
            workers worker10 = CreateWorker("aj@tak.dk", "Stig", "Berthelsen", 29);

            #endregion

            #endregion

            #region sites

            #region Site1
            sites site1 = CreateSite("SiteName1", 88);

            #endregion

            #region Site2
            sites site2 = CreateSite("SiteName2", 89);

            #endregion

            #region Site3
            sites site3 = CreateSite("SiteName3", 90);

            #endregion

            #region Site4
            sites site4 = CreateSite("SiteName4", 91);

            #endregion

            #region Site5
            sites site5 = CreateSite("SiteName5", 92);

            #endregion

            #region Site6
            sites site6 = CreateSite("SiteName6", 93);

            #endregion

            #region Site7
            sites site7 = CreateSite("SiteName7", 94);

            #endregion

            #region Site8
            sites site8 = CreateSite("SiteName8", 95);

            #endregion

            #region Site9
            sites site9 = CreateSite("SiteName9", 96);

            #endregion

            #region Site10
            sites site10 = CreateSite("SiteName10", 97);

            #endregion

            #endregion

            #region units
            units unit = CreateUnit(48, 49, site1, 348);

            #endregion

            #region site_workers
            site_workers site_workers = CreateSiteWorker(55, site1, worker1);

            #endregion

            #endregion
            // Act

            var match = sut.WorkerNameRead((int)worker1.id);

            // Assert

            Assert.AreEqual(worker1.first_name + " " + worker1.last_name, match);


        }
        [Test]
        public void SQL_Worker_WorkerRead_ReadsWorker()
        {
            // Arrance
            #region Arrance

            #region Checklist

            check_lists Cl1 = CreateTemplate("A1", "D1", "caseType1", "WhereItIs", 1, 0);

            #endregion

            #region SubCheckList

            check_lists Cl2 = CreateSubTemplate("A2", "D2", "caseType2", 2, 0, Cl1);

            #endregion

            #region Fields

            #region field1


            fields f1 = CreateField(1, "barcode", Cl2, "e2f4fb", "custom", null, "", "Comment field description",
                5, 1, DbContext.field_types.Where(x => x.field_type == "picture").First(), 0, 0, 1, 0, "Comment field", 1, 55, "55", "0", 0, 0, null, 1, 0,
                0, 0, "", 49);

            #endregion

            #region field2


            fields f2 = CreateField(1, "barcode", Cl2, "f5eafa", "custom", null, "", "showPDf Description",
                45, 1, DbContext.field_types.Where(x => x.field_type == "comment").First(), 0, 1, 0, 0,
                "ShowPdf", 0, 5, "5", "0", 0, 0, null, 0, 0, 0, 0, "", 9);


            #endregion

            #region field3

            fields f3 = CreateField(0, "barcode", Cl2, "f0f8db", "custom", 3, "", "Number Field Description",
                83, 0, DbContext.field_types.Where(x => x.field_type == "picture").First(), 0, 0, 1, 0,
                "Numberfield", 1, 8, "4865", "0", 0, 1, null, 1, 0, 0, 0, "", 1);


            #endregion

            #region field4


            fields f4 = CreateField(1, "barcode", Cl2, "fff6df", "custom", null, "", "date Description",
                84, 0, DbContext.field_types.Where(x => x.field_type == "picture").First(), 0, 0, 1, 0,
                "Date", 1, 666, "41153", "0", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field5

            fields f5 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                85, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field6

            fields f6 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                86, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field7

            fields f7 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                87, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field8

            fields f8 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                88, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field9

            fields f9 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                89, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field10

            fields f10 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                90, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #endregion

            #region Workers

            #region worker1
            workers worker1 = CreateWorker("aa@tak.dk", "Arne", "Jensen", 21);

            #endregion

            #region worker2
            workers worker2 = CreateWorker("ab@tak.dk", "Lasse", "Johansen", 44);

            #endregion

            #region worker3
            workers worker3 = CreateWorker("ac@tak.dk", "Svend", "Jensen", 22);

            #endregion

            #region worker4
            workers worker4 = CreateWorker("ad@tak.dk", "Bjarne", "Nielsen", 23);

            #endregion

            #region worker5
            workers worker5 = CreateWorker("ae@tak.dk", "Ib", "Hansen", 24);

            #endregion

            #region worker6
            workers worker6 = CreateWorker("af@tak.dk", "Hozan", "Aziz", 25);

            #endregion

            #region worker7
            workers worker7 = CreateWorker("ag@tak.dk", "Nicolai", "Peders", 26);

            #endregion

            #region worker8
            workers worker8 = CreateWorker("ah@tak.dk", "Amin", "Safari", 27);

            #endregion

            #region worker9
            workers worker9 = CreateWorker("ai@tak.dk", "Leo", "Rebaz", 28);

            #endregion

            #region worker10
            workers worker10 = CreateWorker("aj@tak.dk", "Stig", "Berthelsen", 29);

            #endregion

            #endregion

            #region sites

            #region Site1
            sites site1 = CreateSite("SiteName1", 88);

            #endregion

            #region Site2
            sites site2 = CreateSite("SiteName2", 89);

            #endregion

            #region Site3
            sites site3 = CreateSite("SiteName3", 90);

            #endregion

            #region Site4
            sites site4 = CreateSite("SiteName4", 91);

            #endregion

            #region Site5
            sites site5 = CreateSite("SiteName5", 92);

            #endregion

            #region Site6
            sites site6 = CreateSite("SiteName6", 93);

            #endregion

            #region Site7
            sites site7 = CreateSite("SiteName7", 94);

            #endregion

            #region Site8
            sites site8 = CreateSite("SiteName8", 95);

            #endregion

            #region Site9
            sites site9 = CreateSite("SiteName9", 96);

            #endregion

            #region Site10
            sites site10 = CreateSite("SiteName10", 97);

            #endregion

            #endregion

            #region units
            units unit = CreateUnit(48, 49, site1, 348);

            #endregion

            #region site_workers
            site_workers site_workers = CreateSiteWorker(55, site1, worker1);

            #endregion

            #endregion
            // Act

            var match = sut.WorkerRead((int)worker1.microting_uid);

            // Assert

            Assert.AreEqual(worker1.microting_uid, match.WorkerUId);
            Assert.AreEqual(worker1.first_name, match.FirstName);



        }
        [Test]
        public void SQL_Worker_WorkerUpdate_UpdatesWorker()
        {
            // Arrance
            #region Arrance

            #region Checklist

            check_lists Cl1 = CreateTemplate("A1", "D1", "caseType1", "WhereItIs", 1, 0);

            #endregion

            #region SubCheckList

            check_lists Cl2 = CreateSubTemplate("A2", "D2", "caseType2", 2, 0, Cl1);

            #endregion

            #region Fields

            #region field1


            fields f1 = CreateField(1, "barcode", Cl2, "e2f4fb", "custom", null, "", "Comment field description",
                5, 1, DbContext.field_types.Where(x => x.field_type == "picture").First(), 0, 0, 1, 0, "Comment field", 1, 55, "55", "0", 0, 0, null, 1, 0,
                0, 0, "", 49);

            #endregion

            #region field2


            fields f2 = CreateField(1, "barcode", Cl2, "f5eafa", "custom", null, "", "showPDf Description",
                45, 1, DbContext.field_types.Where(x => x.field_type == "comment").First(), 0, 1, 0, 0,
                "ShowPdf", 0, 5, "5", "0", 0, 0, null, 0, 0, 0, 0, "", 9);


            #endregion

            #region field3

            fields f3 = CreateField(0, "barcode", Cl2, "f0f8db", "custom", 3, "", "Number Field Description",
                83, 0, DbContext.field_types.Where(x => x.field_type == "picture").First(), 0, 0, 1, 0,
                "Numberfield", 1, 8, "4865", "0", 0, 1, null, 1, 0, 0, 0, "", 1);


            #endregion

            #region field4


            fields f4 = CreateField(1, "barcode", Cl2, "fff6df", "custom", null, "", "date Description",
                84, 0, DbContext.field_types.Where(x => x.field_type == "picture").First(), 0, 0, 1, 0,
                "Date", 1, 666, "41153", "0", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field5

            fields f5 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                85, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field6

            fields f6 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                86, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field7

            fields f7 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                87, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field8

            fields f8 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                88, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field9

            fields f9 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                89, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field10

            fields f10 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                90, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #endregion

            #region Workers

            #region worker1
            workers worker1 = CreateWorker("aa@tak.dk", "Arne", "Jensen", 21);

            #endregion

            #region worker2
            workers worker2 = CreateWorker("ab@tak.dk", "Lasse", "Johansen", 44);

            #endregion

            #region worker3
            workers worker3 = CreateWorker("ac@tak.dk", "Svend", "Jensen", 22);

            #endregion

            #region worker4
            workers worker4 = CreateWorker("ad@tak.dk", "Bjarne", "Nielsen", 23);

            #endregion

            #region worker5
            workers worker5 = CreateWorker("ae@tak.dk", "Ib", "Hansen", 24);

            #endregion

            #region worker6
            workers worker6 = CreateWorker("af@tak.dk", "Hozan", "Aziz", 25);

            #endregion

            #region worker7
            workers worker7 = CreateWorker("ag@tak.dk", "Nicolai", "Peders", 26);

            #endregion

            #region worker8
            workers worker8 = CreateWorker("ah@tak.dk", "Amin", "Safari", 27);

            #endregion

            #region worker9
            workers worker9 = CreateWorker("ai@tak.dk", "Leo", "Rebaz", 28);

            #endregion

            #region worker10
            workers worker10 = CreateWorker("aj@tak.dk", "Stig", "Berthelsen", 29);

            #endregion

            #endregion

            #region sites

            #region Site1
            sites site1 = CreateSite("SiteName1", 88);

            #endregion

            #region Site2
            sites site2 = CreateSite("SiteName2", 89);

            #endregion

            #region Site3
            sites site3 = CreateSite("SiteName3", 90);

            #endregion

            #region Site4
            sites site4 = CreateSite("SiteName4", 91);

            #endregion

            #region Site5
            sites site5 = CreateSite("SiteName5", 92);

            #endregion

            #region Site6
            sites site6 = CreateSite("SiteName6", 93);

            #endregion

            #region Site7
            sites site7 = CreateSite("SiteName7", 94);

            #endregion

            #region Site8
            sites site8 = CreateSite("SiteName8", 95);

            #endregion

            #region Site9
            sites site9 = CreateSite("SiteName9", 96);

            #endregion

            #region Site10
            sites site10 = CreateSite("SiteName10", 97);

            #endregion

            #endregion

            #region units
            units unit = CreateUnit(48, 49, site1, 348);

            #endregion

            #region site_workers
            site_workers site_workers = CreateSiteWorker(55, site1, worker1);

            #endregion

            #endregion
            // Act

            var match = sut.WorkerUpdate(worker1.microting_uid, worker1.first_name, worker1.last_name, worker1.email);


            // Assert

            Assert.True(match);
        }
        [Test]
        public void SQL_Worker_WorkerDelete_DeletesWorker()
        {
            // Arrance
            #region Arrance

            #region Checklist

            check_lists Cl1 = CreateTemplate("A1", "D1", "caseType1", "WhereItIs", 1, 0);

            #endregion

            #region SubCheckList

            check_lists Cl2 = CreateSubTemplate("A2", "D2", "caseType2", 2, 0, Cl1);

            #endregion

            #region Fields

            #region field1


            fields f1 = CreateField(1, "barcode", Cl2, "e2f4fb", "custom", null, "", "Comment field description",
                5, 1, DbContext.field_types.Where(x => x.field_type == "picture").First(), 0, 0, 1, 0, "Comment field", 1, 55, "55", "0", 0, 0, null, 1, 0,
                0, 0, "", 49);

            #endregion

            #region field2


            fields f2 = CreateField(1, "barcode", Cl2, "f5eafa", "custom", null, "", "showPDf Description",
                45, 1, DbContext.field_types.Where(x => x.field_type == "comment").First(), 0, 1, 0, 0,
                "ShowPdf", 0, 5, "5", "0", 0, 0, null, 0, 0, 0, 0, "", 9);


            #endregion

            #region field3

            fields f3 = CreateField(0, "barcode", Cl2, "f0f8db", "custom", 3, "", "Number Field Description",
                83, 0, DbContext.field_types.Where(x => x.field_type == "picture").First(), 0, 0, 1, 0,
                "Numberfield", 1, 8, "4865", "0", 0, 1, null, 1, 0, 0, 0, "", 1);


            #endregion

            #region field4


            fields f4 = CreateField(1, "barcode", Cl2, "fff6df", "custom", null, "", "date Description",
                84, 0, DbContext.field_types.Where(x => x.field_type == "picture").First(), 0, 0, 1, 0,
                "Date", 1, 666, "41153", "0", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field5

            fields f5 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                85, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field6

            fields f6 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                86, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field7

            fields f7 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                87, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field8

            fields f8 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                88, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field9

            fields f9 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                89, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #region field10

            fields f10 = CreateField(0, "barcode", Cl2, "ffe4e4", "custom", null, "", "picture Description",
                90, 0, DbContext.field_types.Where(x => x.field_type == "comment").First(), 1, 0, 1, 0,
                "Picture", 1, 69, "69", "1", 0, 1, null, 0, 1, 0, 0, "", 1);


            #endregion

            #endregion

            #region Workers

            #region worker1
            workers worker1 = CreateWorker("aa@tak.dk", "Arne", "Jensen", 21);

            #endregion

            #region worker2
            workers worker2 = CreateWorker("ab@tak.dk", "Lasse", "Johansen", 44);

            #endregion

            #region worker3
            workers worker3 = CreateWorker("ac@tak.dk", "Svend", "Jensen", 22);

            #endregion

            #region worker4
            workers worker4 = CreateWorker("ad@tak.dk", "Bjarne", "Nielsen", 23);

            #endregion

            #region worker5
            workers worker5 = CreateWorker("ae@tak.dk", "Ib", "Hansen", 24);

            #endregion

            #region worker6
            workers worker6 = CreateWorker("af@tak.dk", "Hozan", "Aziz", 25);

            #endregion

            #region worker7
            workers worker7 = CreateWorker("ag@tak.dk", "Nicolai", "Peders", 26);

            #endregion

            #region worker8
            workers worker8 = CreateWorker("ah@tak.dk", "Amin", "Safari", 27);

            #endregion

            #region worker9
            workers worker9 = CreateWorker("ai@tak.dk", "Leo", "Rebaz", 28);

            #endregion

            #region worker10
            workers worker10 = CreateWorker("aj@tak.dk", "Stig", "Berthelsen", 29);

            #endregion

            #endregion

            #region sites

            #region Site1
            sites site1 = CreateSite("SiteName1", 88);

            #endregion

            #region Site2
            sites site2 = CreateSite("SiteName2", 89);

            #endregion

            #region Site3
            sites site3 = CreateSite("SiteName3", 90);

            #endregion

            #region Site4
            sites site4 = CreateSite("SiteName4", 91);

            #endregion

            #region Site5
            sites site5 = CreateSite("SiteName5", 92);

            #endregion

            #region Site6
            sites site6 = CreateSite("SiteName6", 93);

            #endregion

            #region Site7
            sites site7 = CreateSite("SiteName7", 94);

            #endregion

            #region Site8
            sites site8 = CreateSite("SiteName8", 95);

            #endregion

            #region Site9
            sites site9 = CreateSite("SiteName9", 96);

            #endregion

            #region Site10
            sites site10 = CreateSite("SiteName10", 97);

            #endregion

            #endregion

            #region units
            units unit = CreateUnit(48, 49, site1, 348);

            #endregion

            #region site_workers
            site_workers site_workers = CreateSiteWorker(55, site1, worker1);

            #endregion

            #endregion
            // Act

            var match = sut.WorkerDelete((int)worker1.microting_uid);

            // Assert

            Assert.True(match);

        }
        #endregion

        #region eventhandlers
        public void EventCaseCreated(object sender, EventArgs args)
        {
            // Does nothing for web implementation
        }

        public void EventCaseRetrived(object sender, EventArgs args)
        {
            // Does nothing for web implementation
        }

        public void EventCaseCompleted(object sender, EventArgs args)
        {
            // Does nothing for web implementation
        }

        public void EventCaseDeleted(object sender, EventArgs args)
        {
            // Does nothing for web implementation
        }

        public void EventFileDownloaded(object sender, EventArgs args)
        {
            // Does nothing for web implementation
        }

        public void EventSiteActivated(object sender, EventArgs args)
        {
            // Does nothing for web implementation
        }
        #endregion
    }

}