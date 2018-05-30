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
    public class SqlControllerTestUnit : DbTestFixture
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

        #region unit

        [Test]
        public void SQL_Unit_UnitGetAll_ReturnsAllUnits()
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

            #region Unit1
            units unit1 = CreateUnit(48, 49, site1, 348);
            #endregion

            #region Unit2
            units unit2 = CreateUnit(2, 55, site2, 349);
            #endregion

            #region Unit3
            units unit3 = CreateUnit(3, 51, site3, 350);
            #endregion

            #region Unit4
            units unit4 = CreateUnit(4, 52, site4, 351);
            #endregion

            #region Unit5
            units unit5 = CreateUnit(5, 6, site5, 352);
            #endregion

            #region Unit6
            units unit6 = CreateUnit(6, 85, site6, 353);
            #endregion

            #region Unit7
            units unit7 = CreateUnit(7, 62, site7, 354);
            #endregion

            #region Unit8
            units unit8 = CreateUnit(8, 96, site8, 355);
            #endregion

            #region Unit9
            units unit9 = CreateUnit(9, 69, site9, 356);
            #endregion

            #region Unit10
            units unit10 = CreateUnit(10, 100, site10, 357);
            #endregion


            #endregion

            #region site_workers
            site_workers site_workers = CreateSiteWorker(55, site1, worker1);

            #endregion

            #endregion
            // Act

            var getAllUnits = sut.UnitGetAll();

            // Assert

            Assert.AreEqual(10, getAllUnits.Count());

            Assert.AreEqual(unit1.microting_uid, getAllUnits[0].UnitUId);
            Assert.AreEqual(unit2.microting_uid, getAllUnits[1].UnitUId);
            Assert.AreEqual(unit3.microting_uid, getAllUnits[2].UnitUId);
            Assert.AreEqual(unit4.microting_uid, getAllUnits[3].UnitUId);
            Assert.AreEqual(unit5.microting_uid, getAllUnits[4].UnitUId);
            Assert.AreEqual(unit6.microting_uid, getAllUnits[5].UnitUId);
            Assert.AreEqual(unit7.microting_uid, getAllUnits[6].UnitUId);
            Assert.AreEqual(unit8.microting_uid, getAllUnits[7].UnitUId);
            Assert.AreEqual(unit9.microting_uid, getAllUnits[8].UnitUId);
            Assert.AreEqual(unit10.microting_uid, getAllUnits[9].UnitUId);
        }

        [Test]
        public void SQL_Unit_UnitCreate_CreatesUnit()
        {

            // Arrance
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
            // Act
            var match = sut.UnitCreate(5, 654, 88, (int)site1.microting_uid);
            // Assert
            var units = DbContext.units.AsNoTracking().ToList();

            Assert.NotNull(match);
            Assert.AreEqual(1, units.Count());
            Assert.AreEqual(Constants.WorkflowStates.Created, units[0].workflow_state);


        }

        [Test]
        public void SQL_Unit_UnitRead_ReadsUnit()
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

            #region Unit1
            units unit1 = CreateUnit(48, 49, site1, 348);
            #endregion

            #region Unit2
            units unit2 = CreateUnit(2, 55, site2, 349);
            #endregion

            #region Unit3
            units unit3 = CreateUnit(3, 51, site3, 350);
            #endregion

            #region Unit4
            units unit4 = CreateUnit(4, 52, site4, 351);
            #endregion

            #region Unit5
            units unit5 = CreateUnit(5, 6, site5, 352);
            #endregion

            #region Unit6
            units unit6 = CreateUnit(6, 85, site6, 353);
            #endregion

            #region Unit7
            units unit7 = CreateUnit(7, 62, site7, 354);
            #endregion

            #region Unit8
            units unit8 = CreateUnit(8, 96, site8, 355);
            #endregion

            #region Unit9
            units unit9 = CreateUnit(9, 69, site9, 356);
            #endregion

            #region Unit10
            units unit10 = CreateUnit(10, 100, site10, 357);
            #endregion


            #endregion

            #region site_workers
            site_workers site_workers = CreateSiteWorker(55, site1, worker1);

            #endregion

            #endregion
            // Act

            var match = sut.UnitRead((int)unit1.microting_uid);

            // Assert

            Assert.AreEqual(unit1.microting_uid, match.UnitUId);
            Assert.AreEqual(unit1.customer_no, match.CustomerNo);


        }

        [Test]
        public void SQL_Unit_UnitUpdate_UpdatesUnit()
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

            #region Unit1
            units unit1 = CreateUnit(48, 49, site1, 348);
            #endregion

            #region Unit2
            units unit2 = CreateUnit(2, 55, site2, 349);
            #endregion

            #region Unit3
            units unit3 = CreateUnit(3, 51, site3, 350);
            #endregion

            #region Unit4
            units unit4 = CreateUnit(4, 52, site4, 351);
            #endregion

            #region Unit5
            units unit5 = CreateUnit(5, 6, site5, 352);
            #endregion

            #region Unit6
            units unit6 = CreateUnit(6, 85, site6, 353);
            #endregion

            #region Unit7
            units unit7 = CreateUnit(7, 62, site7, 354);
            #endregion

            #region Unit8
            units unit8 = CreateUnit(8, 96, site8, 355);
            #endregion

            #region Unit9
            units unit9 = CreateUnit(9, 69, site9, 356);
            #endregion

            #region Unit10
            units unit10 = CreateUnit(10, 100, site10, 357);
            #endregion


            #endregion

            #region site_workers
            site_workers site_workers = CreateSiteWorker(55, site1, worker1);

            #endregion

            #endregion
            // Act
            var match = sut.UnitUpdate((int)unit1.microting_uid, (int)unit1.customer_no, (int)unit1.otp_code, (int)unit1.site_id);
            // Assert
            Assert.True(match);
        }

        [Test]
        public void SQL_Unit_UnitDelete_DeletesUnit()
        {  // Arrance
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

            #region Unit1
            units unit1 = CreateUnit(48, 49, site1, 348);
            #endregion

            #region Unit2
            units unit2 = CreateUnit(2, 55, site2, 349);
            #endregion

            #region Unit3
            units unit3 = CreateUnit(3, 51, site3, 350);
            #endregion

            #region Unit4
            units unit4 = CreateUnit(4, 52, site4, 351);
            #endregion

            #region Unit5
            units unit5 = CreateUnit(5, 6, site5, 352);
            #endregion

            #region Unit6
            units unit6 = CreateUnit(6, 85, site6, 353);
            #endregion

            #region Unit7
            units unit7 = CreateUnit(7, 62, site7, 354);
            #endregion

            #region Unit8
            units unit8 = CreateUnit(8, 96, site8, 355);
            #endregion

            #region Unit9
            units unit9 = CreateUnit(9, 69, site9, 356);
            #endregion

            #region Unit10
            units unit10 = CreateUnit(10, 100, site10, 357);
            #endregion


            #endregion

            #region site_workers
            site_workers site_workers = CreateSiteWorker(55, site1, worker1);

            #endregion

            #endregion

            // Act
            var match = sut.UnitDelete((int)unit1.microting_uid);
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