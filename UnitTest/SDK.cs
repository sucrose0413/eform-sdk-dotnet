﻿using eFormCommunicator;
using eFormCore;
using eFormData;
using eFormShared;
using eFormSqlController;
using eFormSubscriber;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;

namespace UnitTest
{
    public class TestContext : IDisposable
    {
        bool useLiveData = false;

        string connectionStringLocal_UnitTest = "Persist Security Info=True;server=localhost;database=microtingMySQL;uid=root;password=1234"; //Uses unit test data
        string connectionStringLocal_LiveData = "Data Source=DESKTOP-7V1APE5\\SQLEXPRESS;Initial Catalog=MicrotingTestNew;Integrated Security=True"; //Uses LIVE data

        #region content
        #region var
        SqlController sqlController;
        string serverConnectionString = "";
        #endregion

        #region once for all tests - build order
        public TestContext()
        {
            try
            {
                if (Environment.MachineName.ToLower().Contains("testing"))
                {
                    serverConnectionString = "Persist Security Info=True;server=localhost;database=microtingMySQL;uid=root;password="; //Uses travis database
                    useLiveData = false;
                }
                else
                {
                    if (useLiveData)
                        serverConnectionString = connectionStringLocal_LiveData;
                    else
                        serverConnectionString = connectionStringLocal_UnitTest;
                }
            }
            catch { }

            sqlController = new SqlController(serverConnectionString);
            AdminTools at = new AdminTools(serverConnectionString);

            if (sqlController.SettingRead(Settings.token) == "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX")
                at.DbSetup("unittest");
        }
        #endregion

        #region once for all tests - teardown
        public void Dispose()
        {
            //sqlController.UnitTest_DeleteDb();
        }
        #endregion

        public string GetConnectionString()
        {
            return serverConnectionString;
        }

        public bool GetUseLiveData()
        {
            return useLiveData;
        }
        #endregion
    }

    [Collection("Database collection")]
    public class SDK
    {
        #region var
        Core core;
        UnitTestCore utCore;
        SqlController sqlController;
        Communicator communicator;
        AdminTools adminTool;
        Tools t = new Tools();

        object _lockTest = new object();
        object _lockFil = new object();

        int siteId1 = 2001;
        int siteId2 = 2002;
        int workerMUId = 666;
        int unitMUId = 345678;

        bool useLiveData = false;

        string token;
        string comAddressApi;
        string comAddressBasic;
        string comOrganizationId;
        string serverConnectionString = "";
        #endregion

        #region con
        public SDK(TestContext testContext)
        {
            serverConnectionString  = testContext.GetConnectionString();
            useLiveData             = testContext.GetUseLiveData();

            if (useLiveData)
            {
                siteId1 = 3818;
                siteId2 = 3823;
                workerMUId = 1778;
                unitMUId = 4938;
            }
        }
        #endregion

        #region prepare and teardown     
        private void TestPrepare(string testName)
        {
            adminTool = new AdminTools(serverConnectionString);
            string temp = adminTool.DbClear();
            if (temp != "")
                throw new Exception("CleanUp failed");

            sqlController = new SqlController(serverConnectionString);
            communicator = new Communicator(sqlController);

            token = sqlController.SettingRead(Settings.token);
            comAddressApi = sqlController.SettingRead(Settings.comAddressApi);
            comAddressBasic = sqlController.SettingRead(Settings.comAddressBasic);
            comOrganizationId = sqlController.SettingRead(Settings.comOrganizationId);

            core = new Core();
            utCore = new UnitTestCore(core);

            core.HandleNotificationNotFound += EventNotificationNotFound;
            core.HandleEventException += EventException;
            core.Start(serverConnectionString);
        }

        private void TestTeardown()
        {
            if (core != null)
                if (core.Running())
                    utCore.Close();
        }
        #endregion

        #region - test 000x virtal basics
        [Fact]
        public void Test000_Basics_1a_MustAlwaysPass()
        {
            lock (_lockTest)
            {
                //Arrange
                bool checkValueA = true;
                bool checkValueB = false;

                //Act
                checkValueB = true;

                //Assert
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test000_Basics_2a_PrepareAndTeardownTestdata()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                bool checkValueA = true;
                bool checkValueB = false;
     
                //Act
                checkValueB = true;

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }
        #endregion

        #region - test 001x core
        [Fact]
        public void Test001_Core_1a_Start_WithNullExpection()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "serverConnectionString is not allowed to be null or empty";
                string checkValueB = "";
                Core core = new Core();

                //Act
                try
                {
                    checkValueB = core.Start(null).ToString();
                }
                catch (Exception ex)
                {
                    checkValueB = ex.InnerException.InnerException.Message;
                }

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test001_Core_1b_Start_WithBlankExpection()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "serverConnectionString is not allowed to be null or empty";
                string checkValueB = "";
                Core core = new Core();

                //Act
                try
                {
                    checkValueB = core.Start("").ToString();
                }
                catch (Exception ex)
                {
                    checkValueB = ex.InnerException.InnerException.Message;
                }

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test001_Core_2a_StartSqlOnly()
        {
            //Arrange
            TestPrepare(t.GetMethodName());
            string checkValueA = "True";
            string checkValueB = "";
            Core core = new Core();

            //Act
            try
            {
                checkValueB = core.StartSqlOnly(serverConnectionString).ToString();
            }
            catch (Exception ex)
            {
                checkValueB = t.PrintException(t.GetMethodName() + " failed", ex);
            }

            //Assert
            TestTeardown();
            Assert.Equal(checkValueA, checkValueB);
        }

        [Fact]
        public void Test001_Core_3a_Start()
        {
            //Arrange
            TestPrepare(t.GetMethodName());
            string checkValueA = "True";
            string checkValueB = "";
            Core core = new Core();

            //Act
            try
            {
                checkValueB = core.Start(serverConnectionString).ToString();
            }
            catch (Exception ex)
            {
                checkValueB = t.PrintException(t.GetMethodName() + " failed", ex);
            }

            //Assert
            TestTeardown();
            Assert.Equal(checkValueA, checkValueB);
        }

        [Fact]
        public void Test001_Core_4a_IsRunning()
        {
            //Arrange
            TestPrepare(t.GetMethodName());
            string checkValueA = "FalseTrue";
            string checkValueB = "";
            Core core = new Core();

            //Act
            try
            {
                checkValueB  = core.Running().ToString();
                               core.Start(serverConnectionString);
                checkValueB += core.Running().ToString();
            }
            catch (Exception ex)
            {
                checkValueB = t.PrintException(t.GetMethodName() + " failed", ex);
            }

            //Assert
            TestTeardown();
            Assert.Equal(checkValueA, checkValueB);
        }

        [Fact]
        public void Test001_Core_5a_Close()
        {
            //Arrange
            TestPrepare(t.GetMethodName());
            string checkValueA = "True";
            string checkValueB = "";
            Core core = new Core();

            //Act
            try
            {
                checkValueB = core.Close().ToString();
            }
            catch (Exception ex)
            {
                checkValueB = t.PrintException(t.GetMethodName() + " failed", ex);
            }

            //Assert
            TestTeardown();
            Assert.Equal(checkValueA, checkValueB);
        }

        [Fact]
        public void Test001_Core_6a_RunningForWhileThenClose()
        {
            //Arrange
            TestPrepare(t.GetMethodName());
            string checkValueA = "FalseTrueTrue";
            string checkValueB = "";
            Core core = new Core();

            //Act
            try
            {
                checkValueB = core.Running().ToString();
                core.Start(serverConnectionString);
                Thread.Sleep(30000);
                checkValueB += core.Running().ToString();
                Thread.Sleep(05000);
                checkValueB += core.Close().ToString();
            }
            catch (Exception ex)
            {
                checkValueB = t.PrintException(t.GetMethodName() + " failed", ex);
            }

            //Assert
            TestTeardown();
            Assert.Equal(checkValueA, checkValueB);
        }

        #endregion

        #region - test 002x xml
        [Fact]
        public void Test002_Xml_1a_XmlImporter()
        {
            lock (_lockTest)
            {
                //Arrange
                string checkValueA;
                string checkValueB;

                //Act
                checkValueA = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Main xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">  <Id>1</Id>  <Label>A container check list</Label>  <DisplayOrder>1</DisplayOrder>  <CheckListFolderName>Main element</CheckListFolderName>  <Repeated>1</Repeated>  <StartDate>11-10-2016</StartDate>  <EndDate>11-10-2017</EndDate>  <Language>en</Language>  <MultiApproval>true</MultiApproval>  <FastNavigation>false</FastNavigation>  <DownloadEntities>false</DownloadEntities>  <ManualSync>true</ManualSync>  <ElementList>    <Element xsi:type=\"DataElement\">      <Id>1</Id>      <Label>Basic list</Label>      <DisplayOrder>1</DisplayOrder>      <Description>Data element</Description>      <ApprovalEnabled>true</ApprovalEnabled>      <ReviewEnabled>true</ReviewEnabled>      <DoneButtonEnabled>true</DoneButtonEnabled>      <ExtraFieldsEnabled>false</ExtraFieldsEnabled>      <PinkBarText />      <DataItemList>        <DataItem xsi:type=\"Number\">          <Id>1</Id>          <Mandatory>false</Mandatory>          <ReadOnly>false</ReadOnly>          <Label>Number field</Label>          <Description>this is a description</Description>          <Color>e2f4fb</Color>          <DisplayOrder>1</DisplayOrder>          <MinValue>0</MinValue>          <MaxValue>1000</MaxValue>          <DefaultValue>0</DefaultValue>          <DecimalCount>0</DecimalCount>          <UnitName />        </DataItem>        <DataItem xsi:type=\"Text\">          <Id>2</Id>          <Mandatory>false</Mandatory>          <ReadOnly>false</ReadOnly>          <Label>Text field</Label>          <Description>this is a description bla</Description>          <Color>e2f4fb</Color>          <DisplayOrder>8</DisplayOrder>          <Value>true</Value>          <MaxLength>100</MaxLength>          <GeolocationEnabled>false</GeolocationEnabled>          <GeolocationForced>false</GeolocationForced>          <GeolocationHidden>true</GeolocationHidden>          <BarcodeEnabled>false</BarcodeEnabled>          <BarcodeType />        </DataItem>        <DataItem xsi:type=\"Comment\">          <Id>3</Id>          <Mandatory>false</Mandatory>          <ReadOnly>false</ReadOnly>          <Label>Comment field</Label>          <Description>this is a description</Description>          <Color>e2f4fb</Color>          <DisplayOrder>3</DisplayOrder>          <Value>value</Value>          <Maxlength>10000</Maxlength>          <SplitScreen>false</SplitScreen>        </DataItem>        <DataItem xsi:type=\"Picture\">          <Id>4</Id>          <Mandatory>false</Mandatory>          <ReadOnly>false</ReadOnly>          <Label>Picture field</Label>          <Description>this is a description</Description>          <Color>e2f4fb</Color>          <DisplayOrder>4</DisplayOrder>          <Multi>1</Multi>          <GeolocationEnabled>true</GeolocationEnabled>        </DataItem>        <DataItem xsi:type=\"Check_Box\">          <Id>5</Id>          <Mandatory>false</Mandatory>          <ReadOnly>true</ReadOnly>          <Label>Check box</Label>          <Description>this is a description</Description>          <Color>e2f4fb</Color>          <DisplayOrder>15</DisplayOrder>          <DefaultValue>true</DefaultValue>          <Selected>true</Selected>        </DataItem>        <DataItem xsi:type=\"Date\">          <Id>6</Id>          <Mandatory>false</Mandatory>          <ReadOnly>false</ReadOnly>          <Label>Date field</Label>          <Description>this is a description</Description>          <Color>e2f4fb</Color>          <DisplayOrder>16</DisplayOrder>          <DefaultValue>11-10-2016 15:20:51</DefaultValue>          <MaxValue>2016-10-11T15:20:51.5733094+02:00</MaxValue>          <MinValue>2016-10-11T15:20:51.5733094+02:00</MinValue>        </DataItem>        <DataItem xsi:type=\"None\">          <Id>7</Id>          <Mandatory>false</Mandatory>          <ReadOnly>false</ReadOnly>          <Label>None field, only shows text</Label>          <Description>this is a description</Description>          <Color>e2f4fb</Color>          <DisplayOrder>7</DisplayOrder>        </DataItem>      </DataItemList>    </Element>  </ElementList>  <PushMessageTitle />  <PushMessageBody /></Main>";

                checkValueB = LoadFil("xml.txt");
                checkValueB = checkValueA.Replace("\n", String.Empty).Replace("\r", String.Empty).Replace("\t", String.Empty);

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test002_Xml_2a_XmlConverter()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA;
                string checkValueB;

                //Act
                checkValueA = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Main xmlns:>  <Id>1</Id>  <Label>A container check list</Label>  <DisplayOrder>1</DisplayOrder>  <CheckListFolderName>Main element</CheckListFolderName>  <Repeated>1</Repeated>  <StartDate>2016-10-11 00:00:00</StartDate>  <EndDate>2017-10-11 00:00:00</EndDate>  <Language>en</Language>  <MultiApproval>true</MultiApproval>  <FastNavigation>false</FastNavigation>  <DownloadEntities>false</DownloadEntities>  <ManualSync>true</ManualSync>  <ElementList>    <Element xsi:type=\"DataElement\">      <Id>1</Id>      <Label>Basic list</Label>      <DisplayOrder>1</DisplayOrder>      <Description><![CDATA[Data element]]></Description>      <ApprovalEnabled>true</ApprovalEnabled>      <ReviewEnabled>true</ReviewEnabled>      <DoneButtonEnabled>true</DoneButtonEnabled>      <ExtraFieldsEnabled>false</ExtraFieldsEnabled>      <PinkBarText />      <DataItemGroupList />      <DataItemList>        <DataItem xsi:type=\"Number\">          <Id>1</Id>          <Mandatory>false</Mandatory>          <ReadOnly>false</ReadOnly>          <Label>Number field</Label>          <Description><![CDATA[this is a description]]></Description>          <Color>e2f4fb</Color>          <DisplayOrder>1</DisplayOrder>          <MinValue>0</MinValue>          <MaxValue>1000</MaxValue>          <DefaultValue>0</DefaultValue>          <DecimalCount>0</DecimalCount>          <UnitName />        </DataItem>        <DataItem xsi:type=\"Text\">          <Id>2</Id>          <Mandatory>false</Mandatory>          <ReadOnly>false</ReadOnly>          <Label>Text field</Label>          <Description><![CDATA[this is a description bla]]></Description>          <Color>e2f4fb</Color>          <DisplayOrder>8</DisplayOrder>          <Value>true</Value>          <MaxLength>100</MaxLength>          <GeolocationEnabled>false</GeolocationEnabled>          <GeolocationForced>false</GeolocationForced>          <GeolocationHidden>true</GeolocationHidden>          <BarcodeEnabled>false</BarcodeEnabled>          <BarcodeType />        </DataItem>        <DataItem xsi:type=\"Comment\">          <Id>3</Id>          <Mandatory>false</Mandatory>          <ReadOnly>false</ReadOnly>          <Label>Comment field</Label>          <Description><![CDATA[this is a description]]></Description>          <Color>e2f4fb</Color>          <DisplayOrder>3</DisplayOrder>          <Value>value</Value>          <Maxlength>10000</Maxlength>          <SplitScreen>false</SplitScreen>        </DataItem>        <DataItem xsi:type=\"Picture\">          <Id>4</Id>          <Mandatory>false</Mandatory>          <ReadOnly>false</ReadOnly>          <Label>Picture field</Label>          <Description><![CDATA[this is a description]]></Description>          <Color>e2f4fb</Color>          <DisplayOrder>4</DisplayOrder>          <Multi>1</Multi>          <GeolocationEnabled>true</GeolocationEnabled>        </DataItem>        <DataItem xsi:type=\"CheckBox\">          <Id>5</Id>          <Mandatory>false</Mandatory>          <ReadOnly>true</ReadOnly>          <Label>Check box</Label>          <Description><![CDATA[this is a description]]></Description>          <Color>e2f4fb</Color>          <DisplayOrder>15</DisplayOrder>          <DefaultValue>true</DefaultValue>          <Selected>true</Selected>        </DataItem>        <DataItem xsi:type=\"Date\">          <Id>6</Id>          <Mandatory>false</Mandatory>          <ReadOnly>false</ReadOnly>          <Label>Date field</Label>          <Description><![CDATA[this is a description]]></Description>          <Color>e2f4fb</Color>          <DisplayOrder>16</DisplayOrder>          <DefaultValue>11-10-2016 15:20:51</DefaultValue>          <MaxValue>2016-10-11</MaxValue>          <MinValue>2016-10-11</MinValue>        </DataItem>        <DataItem xsi:type=\"None\">          <Id>7</Id>          <Mandatory>false</Mandatory>          <ReadOnly>false</ReadOnly>          <Label>None field, only shows text</Label>          <Description><![CDATA[this is a description]]></Description>          <Color>e2f4fb</Color>          <DisplayOrder>7</DisplayOrder>        </DataItem>      </DataItemList>    </Element>  </ElementList>  <PushMessageTitle />  <PushMessageBody /></Main>";

                checkValueB = LoadFil("xml.txt");
                MainElement main = core.TemplateFromXml(checkValueB);
                checkValueB = main.ClassToXml();
                checkValueB = t.LocateReplace(checkValueB, "<Main xmlns:", ">", "");
                checkValueB = checkValueB.Replace("\n", String.Empty).Replace("\r", String.Empty).Replace("\t", String.Empty);

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test002_Xml_3a_TemplateCreate()
        {
             lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                int checkValueA;
                int checkValueB;
                MainElement main;
                string xmlStr;

                //Act
                checkValueA = -1;

                checkValueB = -1;
                xmlStr = LoadFil("xml.txt");
                main = core.TemplateFromXml(xmlStr);
                checkValueB = core.TemplateCreate(main);

                //Assert
                TestTeardown();
                Assert.NotEqual(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test002_Xml_4a_TemplateRead()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA;
                string checkValueB;
                MainElement main;
                int templatId;
                string xmlStr;

                //Act
                xmlStr = LoadFil("xml.txt");
                main = core.TemplateFromXml(xmlStr);
                checkValueA = main.ClassToXml();
                checkValueA = ClearXml(checkValueA);

                templatId = core.TemplateCreate(main);
                main = core.TemplateRead(templatId);
                checkValueB = main.ClassToXml();
                checkValueB = ClearXml(checkValueB);

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }
        #endregion

        #region - test 003x - communicator
        [Fact]
        public void Test003_Communicator_1a_PostXml()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                bool checkValueA = true;
                bool checkValueB;
                string xmlStr;
                MainElement main = new MainElement();
                Communicator com = new Communicator(sqlController);

                //Act
                xmlStr = LoadFil("xml.txt");
                main = core.TemplateFromXml(xmlStr);
                string responseStr = com.PostXml(xmlStr, siteId1);
                checkValueB = responseStr.Contains("<Response><Value type=\"success\">");

                if (checkValueB)
                {
                    string mUId = t.Locate(responseStr, "<Value type=\"success\">", "</");
                    WaitForAvailableMicroting(mUId);
                }

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test003_Communicator_2a_CheckStatus()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                bool checkValueA = true;
                bool checkValueB = false;
                string xmlStr;
                MainElement main = new MainElement();
                Communicator com = new Communicator(sqlController);

                //Act
                xmlStr = LoadFil("xml.txt");
                main = core.TemplateFromXml(xmlStr);
                string responseStr = com.PostXml(xmlStr, siteId1);

                if (responseStr.Contains("<Response><Value type=\"success\">"))
                {
                    string mUId = t.Locate(responseStr, "<Value type=\"success\">", "</");
                    responseStr = com.CheckStatus(mUId, siteId1);

                    if (responseStr.Contains("<Response><Value type=\"success\">") || 
                        responseStr.Contains("<Response><Value type=\"received\">"))
                        checkValueB = WaitForAvailableMicroting(mUId);
                }

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test003_Communicator_3a_Retrieve()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                bool checkValueA = true;
                bool checkValueB = false;
                string xmlStr;
                MainElement main = new MainElement();
                Communicator com = new Communicator(sqlController);

                //Act
                xmlStr = LoadFil("xml.txt");
                main = core.TemplateFromXml(xmlStr);
                string responseStr = com.PostXml(xmlStr, siteId1);

                if (responseStr.Contains("<Response><Value type=\"success\">"))
                {
                    string mUId = t.Locate(responseStr, "<Value type=\"success\">", "</");
                    checkValueB = WaitForAvailableMicroting(mUId);
                }

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test003_Communicator_3b_RetrieveFromId()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                bool checkValueA = true;
                bool checkValueB = false;
                string xmlStr;
                MainElement main = new MainElement();
                Communicator com = new Communicator(sqlController);

                //Act
                xmlStr = LoadFil("xml.txt");
                main = core.TemplateFromXml(xmlStr);
                string responseStr = com.PostXml(xmlStr, siteId1);
                string mUId = "";

                if (responseStr.Contains("<Response><Value type=\"success\">"))
                {
                    mUId = t.Locate(responseStr, "<Value type=\"success\">", "</");
                    responseStr = com.RetrieveFromId(mUId, siteId1, "");

                    if (responseStr.Contains("<Response><Value type="))
                    {
                        checkValueB = true;
                        WaitForAvailableMicroting(mUId);
                    }
                }

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test003_Communicator_4a_Delete()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                bool checkValueA = true;
                bool checkValueB = false;
                string xmlStr;
                MainElement main = new MainElement();
                Communicator com = new Communicator(sqlController);

                //Act
                xmlStr = LoadFil("xml.txt");
                main = core.TemplateFromXml(xmlStr);
                string responseStr = com.PostXml(xmlStr, siteId1);

                if (responseStr.Contains("<Response><Value type=\"success\">"))
                {
                    string mUId = t.Locate(responseStr, "<Value type=\"success\">", "</");
                    WaitForAvailableMicroting(mUId);

                    if (responseStr.Contains("<Response><Value type=\"success\">"))
                    {
                        checkValueB = true;
                    }
                }

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }
        #endregion

        #region - test 004x - request (XML)
        [Fact]
        public void Test004_Request_1a_ClassToXml()
        {
            lock (_lockTest)
            {
                //Arrange
                string checkValueA = ClearXml(LoadFil("requestXmlFromClass.txt"));
                string checkValueB = "";
                DateTime startDate = DateTime.Now;
                DateTime endDate = DateTime.Now.AddYears(1);
                MainElement main = new MainElement(1, "Sample 3", 1, "Main element", 1, startDate, endDate, "en", true, false, false, true, "", "", "", new List<Element>());
                #region main populated
                GroupElement g1 = new GroupElement(11, "Group of advanced check lists", 1, "Group element", false, false, false, false, "", new List<Element>());
                main.ElementList.Add(g1);


                DataElement e1 = new DataElement(21, "Advanced list", 1, "Data element", true, true, true, false, "", null, new List<DataItem>());
                g1.ElementList.Add(e1);

                DataElement e2 = new DataElement(22, "Advanced list", 2, "Data element", true, true, true, false, "", null, new List<DataItem>());
                g1.ElementList.Add(e2);

                DataElement e3 = new DataElement(23, "Advanced list", 3, "Data element", true, true, true, false, "", null, new List<DataItem>());
                g1.ElementList.Add(e3);


                List<KeyValuePair> singleKeyValuePairList = new List<KeyValuePair>();
                singleKeyValuePairList.Add(new KeyValuePair("1", "option 1", true, "1"));
                singleKeyValuePairList.Add(new KeyValuePair("2", "option 2", false, "2"));
                singleKeyValuePairList.Add(new KeyValuePair("3", "option 3", false, "3"));

                List<KeyValuePair> multiKeyValuePairList = new List<KeyValuePair>();
                multiKeyValuePairList.Add(new KeyValuePair("1", "option 1", true, "1"));
                multiKeyValuePairList.Add(new KeyValuePair("2", "option 2", true, "2"));
                multiKeyValuePairList.Add(new KeyValuePair("3", "option 3", false, "3"));

                e1.DataItemList.Add(new SingleSelect(1, false, false, "Single select field", "this is a description", "e2f4fb", 1, false, singleKeyValuePairList));
                e1.DataItemList.Add(new MultiSelect(2, false, false, "Multi select field", "this is a description", "e2f4fb", 2, false, multiKeyValuePairList));
                e1.DataItemList.Add(new Audio(3, false, false, "Audio field", "this is a description", "e2f4fb", 3, false, 1));
                e1.DataItemList.Add(new Comment(5, false, false, "Comment field", "this is a description", "e2f4fb", 5, false, "value", 10000, false));

                e2.DataItemList.Add(new Number(1, false, false, "Number field", "this is a description", "e2f4fb", 1, false, 0, 1000, 2, 0, ""));
                e2.DataItemList.Add(new Text(2, false, false, "Text field", "this is a description bla", "e2f4fb", 2, false, "true", 100, false, false, true, false, ""));
                e2.DataItemList.Add(new Comment(3, false, false, "Comment field", "this is a description", "e2f4fb", 3, false, "value", 10000, false));
                e2.DataItemList.Add(new Picture(4, false, false, "Picture field", "this is a description", "e2f4fb", 4, false, 1, true));
                e2.DataItemList.Add(new CheckBox(5, false, false, "Check box", "this is a description", "e2f4fb", 5, false, true, true));
                e2.DataItemList.Add(new Date(6, false, false, "Date field", "this is a description", "e2f4fb", 6, false, startDate, startDate, startDate.ToString()));
                e2.DataItemList.Add(new None(7, false, false, "None field, only shows text", "this is a description", "e2f4fb", 7, false));
                e2.DataItemList.Add(new eFormData.Timer(8, false, false, "Timer", "this is a description", "e2f4fb", 8, false, false));
                e2.DataItemList.Add(new Signature(9, false, false, "Signature", "this is a description", "e2f4fb", 9, false));

                e3.DataItemList.Add(new CheckBox(1, true, false, "You are sure?", "Verify please", "e2f4fb", 1, false, false, false));
                #endregion

                //Act
                string xml = main.ClassToXml();
                checkValueB = ClearXml(xml);
                checkValueA = t.LocateReplace(checkValueA, "<Main xmlns:", ">", "");
                checkValueB = t.LocateReplace(checkValueB, "<Main xmlns:", ">", "");

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test004_Request_2a_XmlToClass()
        {
            lock (_lockTest)
            {
                //Arrange
                string checkValueA = ClearXml(LoadFil("requestXmlFromXml.txt"));
                string checkValueB = LoadFil("requestXmlFromClass.txt");
                MainElement main = new MainElement();

                //Act
                main = main.XmlToClass(checkValueB);
                checkValueB = ClearXml(main.ClassToXml());
                checkValueA = t.LocateReplace(checkValueA, "<Main xmlns:", ">", "");
                checkValueB = t.LocateReplace(checkValueB, "<Main xmlns:", ">", "");

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }
        #endregion

        #region - test 005x - response (XML)
        [Fact]
        public void Test005_Response_1a_XmlToClassSimple()
        {
            lock (_lockTest)
            {
                //Arrange
                string responseStr = LoadFil("responseXmlFromXml.txt");
                string checkValueA1 = "903390";
                string checkValueB1 = "";
                Response.ResponseTypes checkValueA2 = Response.ResponseTypes.Success;
                Response.ResponseTypes checkValueB2;
                Response resp = new Response();

                //Act
                resp = resp.XmlToClass(responseStr);
                checkValueB1 = resp.Value;
                checkValueB2 = resp.Type;


                //Assert
                TestTeardown();
                Assert.Equal(checkValueA1, checkValueB1);
                Assert.Equal(checkValueA2, checkValueB2);
            }
        }

        [Fact]
        public void Test005_Response_2a_XmlToClassExt()
        {
            lock (_lockTest)
            {
                //Arrange
                string responseStr = LoadFil("responseXmlFromXmlExt.txt");

                string checkValueA1 = "903392";
                string checkValueB1 = "";

                Response.ResponseTypes checkValueA2 = Response.ResponseTypes.Success;
                Response.ResponseTypes checkValueB2;

                string checkValueA3 = "1749";
                string checkValueB3 = "";

                string checkValueA4 = "approved";
                string checkValueB4 = "";

                string checkValueA5 = "42";
                string checkValueB5 = "";

                string checkValueA6 = "2017-10-07";
                string checkValueB6 = "";

                Response resp = new Response();

                //Act
                resp = resp.XmlToClass(responseStr);
                checkValueB1 = resp.Value;
                checkValueB2 = resp.Type;
                checkValueB3 = resp.Checks[0].UnitId;
                checkValueB4 = resp.Checks[0].ElementList[0].Status;
                checkValueB5 = resp.Checks[0].ElementList[0].DataItemList[0].Value.InderValue;
                checkValueB6 = resp.Checks[0].ElementList[0].DataItemList[5].Value.InderValue;

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA1, checkValueB1);
                Assert.Equal(checkValueA2, checkValueB2);
                Assert.Equal(checkValueA3, checkValueB3);
                Assert.Equal(checkValueA4, checkValueB4);
                Assert.Equal(checkValueA5, checkValueB5);
                Assert.Equal(checkValueA6, checkValueB6);
            }
        }
        #endregion

        #region - test 006x - sqlController (Templat and Case)
        [Fact]
        public void Test006_SqlController_1a_TemplateCreateAndRead()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = ClearXml(LoadFil("requestXmlFromXml.txt"));
                string checkValueB = LoadFil("requestXmlFromClass.txt");
                int templatId = -1;
                MainElement main = new MainElement();

                //Act
                main = main.XmlToClass(checkValueB);
                templatId = sqlController.TemplateCreate(main);
                main = sqlController.TemplateRead(templatId);
                checkValueB = ClearXml(main.ClassToXml());
                checkValueA = t.LocateReplace(checkValueA, "<Main xmlns:", ">", "");
                checkValueB = t.LocateReplace(checkValueB, "<Main xmlns:", ">", "");

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test006_SqlController_2a_CaseCreateAndRead()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValue1A = "created";
                string checkValue1B = "";
                int checkValue2A = 66;
                int checkValue2B = 0;
                int templatId = -1;
                MainElement main = new MainElement();

                //Act
                main = main.XmlToClass(LoadFil("requestXmlFromClass.txt"));
                templatId = sqlController.TemplateCreate(main);
                main = sqlController.TemplateRead(templatId);

                List<int> siteIds = new List<int>();
                siteIds.Add(siteId1);

                sqlController.CaseCreate(templatId, siteId1, "696969", "0", "testCase", "", DateTime.Now);

                cases aCase = sqlController.CaseReadFull("696969", "0");
                checkValue1B = aCase.workflow_state;
                checkValue2B = (int)aCase.status;

                //Assert
                TestTeardown();
                Assert.Equal(checkValue1A, checkValue1B);
                Assert.Equal(checkValue2A, checkValue2B);
            }
        }

        [Fact]
        public void Test006_SqlController_3a_CaseDelete()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValue1A = "removed";
                string checkValue1B = "";
                int checkValue2A = 66;
                int checkValue2B = 0;
                int templatId = -1;
                MainElement main = new MainElement();

                //Act
                main = main.XmlToClass(LoadFil("requestXmlFromClass.txt"));
                templatId = sqlController.TemplateCreate(main);
                main = sqlController.TemplateRead(templatId);

                List<int> siteIds = new List<int>();
                siteIds.Add(siteId1);

                sqlController.CaseCreate(templatId, siteId1, "696969", "1", "testCase", "", DateTime.Now);

                sqlController.CaseDelete("696969");
                cases aCase = sqlController.CaseReadFull("696969", "1");
                checkValue1B = aCase.workflow_state;
                checkValue2B = (int)aCase.status;

                //Assert
                TestTeardown();
                Assert.Equal(checkValue1A, checkValue1B);
                Assert.Equal(checkValue2A, checkValue2B);
            }
        }

        [Fact]
        public void Test006_SqlController_4a_CaseUpdate()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValue1A = "created";
                string checkValue1B = "";
                int checkValue2A = 100;
                int checkValue2B = 0;
                int templatId = -1;
                MainElement main = new MainElement();

                //Act
                main = main.XmlToClass(LoadFil("requestXmlFromClass.txt"));
                templatId = sqlController.TemplateCreate(main);
                main = sqlController.TemplateRead(templatId);

                List<int> siteIds = new List<int>();
                siteIds.Add(siteId1);

                sqlController.CaseCreate(templatId, siteId1, "696969", null, "testCase", "", DateTime.Now);
                sqlController.CaseUpdateRetrived("696969");
                sqlController.CaseUpdateCompleted("696969", "2", DateTime.Now, workerMUId, unitMUId);

                cases aCase = sqlController.CaseReadFull("696969", "2");
                checkValue1B = aCase.workflow_state;
                checkValue2B = (int)aCase.status;

                //Assert
                TestTeardown();
                Assert.Equal(checkValue1A, checkValue1B);
                Assert.Equal(checkValue2A, checkValue2B);
            }
        }

        [Fact]
        public void Test006_SqlController_5a_CaseRetract()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValue1A = "retracted";
                string checkValue1B = "";
                int checkValue2A = 66;
                int checkValue2B = 0;
                int templatId = -1;
                MainElement main = new MainElement();

                //Act
                main = main.XmlToClass(LoadFil("requestXmlFromClass.txt"));
                templatId = sqlController.TemplateCreate(main);
                main = sqlController.TemplateRead(templatId);

                List<int> siteIds = new List<int>();
                siteIds.Add(siteId1);

                sqlController.CaseCreate(templatId, siteId1, "696969", "3", "testCase", "", DateTime.Now);
                sqlController.CaseRetract("696969", "3");
                cases aCase = sqlController.CaseReadFull("696969", "3");
                checkValue1B = aCase.workflow_state;
                checkValue2B = (int)aCase.status;

                //Assert
                TestTeardown();
                Assert.Equal(checkValue1A, checkValue1B);
                Assert.Equal(checkValue2A, checkValue2B);
            }
        }
        #endregion

        #region - test 007x - subscriber
        [Fact]
        public void Test007_Subscriber_1a_StartAndClose()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "TrueFalse";
                string checkValueB = "";
                Subscriber subS = new Subscriber(sqlController, null);

                //Act
                subS.Start();
                checkValueB = subS.IsActive().ToString();
                subS.Close();
                checkValueB += subS.IsActive().ToString();

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test007_Subscriber_2a_LacksName()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "not_found = True";
                string checkValueB = "";

                //Act
                sqlController.NotificationCreate(DateTime.Now.ToLongTimeString(), "not in db", "check_status");
                for (int i = 0; i < 100; i++)
                {
                    Thread.Sleep(100);
                    var lst = sqlController.UnitTest_FindAllNotifications();
                    if (lst[0].workflow_state == "not_found")
                    {
                        checkValueB = "not_found = True";
                        break;
                    }
                }

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }
        #endregion

        #region - test 008x - core (Case)
        [Fact]
        public void Test008_Core_1a_CaseCreate()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                int checkValueA = 1;
                int checkValueB;
                int templatId = -1;
                MainElement main = new MainElement();

                //Act
                main = main.XmlToClass(LoadFil("requestXmlFromClass.txt"));
                templatId = core.TemplateCreate(main);
                main = core.TemplateRead(templatId);

                core.CaseCreate(main, "", siteId1);
                List<string> mUIds = WaitForAvailableDB();
                checkValueB = mUIds.Count;

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test008_Core_2a_CaseDelete()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                bool checkValueA = true;
                bool checkValueB;
                int templatId = -1;
                MainElement main = new MainElement();

                //Act
                main = main.XmlToClass(LoadFil("requestXmlFromClass.txt"));
                templatId = core.TemplateCreate(main);
                main = core.TemplateRead(templatId);

                string temp = core.CaseCreate(main, "", siteId1);
                List<string> mUIds = WaitForAvailableDB();

                WaitForAvailableMicroting(mUIds[0]);
                checkValueB = core.CaseDelete(mUIds[0]);

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test008_Core_3a_CaseCreateReversed()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "1True";
                string checkValueB;
                int templatId = -1;
                MainElement main = new MainElement();

                //Act
                main = main.XmlToClass(LoadFil("requestXmlFromClass.txt"));
                templatId = core.TemplateCreate(main);
                main = core.TemplateRead(templatId);

                List<int> lstSiteIds = new List<int>();
                lstSiteIds.Add(siteId1);

                core.CaseCreate(main, "", lstSiteIds, "custom");
                List<string> mUIds = WaitForAvailableDB();

                checkValueB = mUIds.Count.ToString();
                checkValueB += WaitForAvailableMicroting(mUIds[0]).ToString();

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test008_Core_4a_CaseLookup()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                Case_Dto checkValueA = new Case_Dto();
                Case_Dto checkValueB;
                int templatId = -1;
                MainElement main = new MainElement();

                //Act
                main = main.XmlToClass(LoadFil("requestXmlFromClass.txt"));
                templatId = core.TemplateCreate(main);
                main = core.TemplateRead(templatId);

                core.CaseCreate(main, "", siteId1);
                List<string> mUIds = WaitForAvailableDB();

                WaitForAvailableMicroting(mUIds[0]);
                checkValueB = core.CaseLookupMUId(mUIds[0]);

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA.GetType(), checkValueB.GetType());
            }
        }

        [Fact]
        public void Test008_Core_5a_Close()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                bool checkValueA = true;
                bool checkValueB = false;

                //Act
                checkValueB = core.Close();

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }
        #endregion

        #region - test 009x - entity
        [Fact]
        public void Test009_Entity_1a_EntityGroupCreate_EntitySearch()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "";
                EntityGroup checkValueB;

                //Act
                checkValueB = core.EntityGroupCreate("EntitySearch", "MyTest");

                //Assert
                TestTeardown();
                Assert.NotEqual(checkValueA, checkValueB.ToString());
            }
        }

        [Fact]
        public void Test009_Entity_1b_EntityGroupCreate_EntitySelect()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "";
                EntityGroup checkValueB;

                //Act
                checkValueB = core.EntityGroupCreate("EntitySelect", "MyTest");

                //Assert
                TestTeardown();
                Assert.NotEqual(checkValueA, checkValueB.ToString());
            }
        }

        [Fact]
        public void Test009_Entity_2a_EntityGroupUpdate_NotUnique()
        {
             lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "Failed";
                string checkValueB;


                //Act
                EntityGroup peG = core.EntityGroupCreate("EntitySearch", "MyTest");

                List<EntityItem> lst = new List<EntityItem>();
                EntityGroup eG = new EntityGroup(peG.Id, "Group", "EntitySearch", peG.EntityGroupMUId, lst, "created", DateTime.Now, DateTime.Now);
                lst.Add(new EntityItem("Item1", "description", "1", "created"));
                lst.Add(new EntityItem("Item2", "description", "4", "created"));
                lst.Add(new EntityItem("Item3", "description", "3", "created"));
                lst.Add(new EntityItem("Item4", "description", "4", "created"));

                try
                {
                    core.EntityGroupUpdate(eG);
                    checkValueB = "Passed, which it should not";
                }
                catch
                {
                    checkValueB = "Failed";
                }

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test009_Entity_3a_EntityGroupUpdateAndRead_EntitySearch()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "Item2New5Item6";
                string checkValueB;
                EntityGroup oEG;

                //Act
                oEG = core.EntityGroupCreate("EntitySearch", "MyTest");
                List<EntityItem> lst = new List<EntityItem>();
                EntityGroup eG = new EntityGroup(oEG.Id, "Group", "EntitySearch", oEG.EntityGroupMUId, lst, "created", DateTime.Now, DateTime.Now);
                lst.Add(new EntityItem("Item1", "description", "1", "created"));
                lst.Add(new EntityItem("Item2", "description", "2", "created"));
                lst.Add(new EntityItem("Item3", "description", "3", "created"));
                lst.Add(new EntityItem("Item4", "description", "4", "created"));
                core.EntityGroupUpdate(eG);

                var tempi = core.EntityGroupRead(oEG.EntityGroupMUId);
                checkValueB = tempi.EntityGroupItemLst[1].Name;

                tempi.EntityGroupItemLst[2].Name = "New";
                tempi.EntityGroupItemLst.Add(new EntityItem("Item5", "added", "5", "created"));
                tempi.EntityGroupItemLst.RemoveAt(3);
                tempi.EntityGroupItemLst.Add(new EntityItem("Item6", "added", "6", "created"));
                core.EntityGroupUpdate(tempi);

                var tempii = core.EntityGroupRead(oEG.EntityGroupMUId);
                checkValueB += tempi.EntityGroupItemLst[2].Name;
                checkValueB += tempi.EntityGroupItemLst.Count;
                checkValueB += tempi.EntityGroupItemLst[4].Name;

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test009_Entity_3b_EntityGroupUpdateAndRead_EntitySelect()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "Item2New5Item6True";
                string checkValueB;
                EntityGroup oEG;

                //Act
                oEG = core.EntityGroupCreate("EntitySelect", "MyTest");
                List<EntityItem> lst = new List<EntityItem>();
                EntityGroup eG = new EntityGroup(oEG.Id, "Group", "EntitySelect", oEG.EntityGroupMUId, lst, "created", DateTime.Now, DateTime.Now);
                lst.Add(new EntityItem("Item1", "description & more", "1", "created"));
                lst.Add(new EntityItem("Item2", "description", "2", "created"));
                lst.Add(new EntityItem("Item3 & more", "description", "3", "created"));
                lst.Add(new EntityItem("Item4", "description", "4", "created"));
                core.EntityGroupUpdate(eG);

                var tempi = core.EntityGroupRead(oEG.EntityGroupMUId);
                checkValueB = tempi.EntityGroupItemLst[1].Name;

                tempi.EntityGroupItemLst[2].Name = "New";
                tempi.EntityGroupItemLst.Add(new EntityItem("Item5", "added", "5", "created"));
                tempi.EntityGroupItemLst.RemoveAt(3);
                tempi.EntityGroupItemLst.Add(new EntityItem("Item6", "added", "6", "created"));
                core.EntityGroupUpdate(tempi);
                Thread.Sleep(250);

                var tempii = core.EntityGroupRead(oEG.EntityGroupMUId);
                checkValueB += tempi.EntityGroupItemLst[2].Name;
                checkValueB += tempi.EntityGroupItemLst.Count;
                checkValueB += tempi.EntityGroupItemLst[4].Name;
                checkValueB += sqlController.UnitTest_EntitiesAllSynced(oEG.EntityGroupMUId);

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test009_Entity_4a_EntityGroupItemRemoveAndAddAgain_EntitySearch()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "1st insert2nd insertTrue";
                string checkValueB;
                EntityGroup oEG;

                //Act
                oEG = core.EntityGroupCreate("EntitySearch", "MyTest");
                List<EntityItem> lst = new List<EntityItem>();
                EntityGroup eG = new EntityGroup(oEG.Id, "Group", "EntitySearch", oEG.EntityGroupMUId, lst, "created", DateTime.Now, DateTime.Now);
                lst.Add(new EntityItem("Item1", "1st insert", "3", "created"));
                lst.Add(new EntityItem("Item2", "other", "4", "created"));

                core.EntityGroupUpdate(eG);

                var tempi = core.EntityGroupRead(oEG.EntityGroupMUId);
                checkValueB = tempi.EntityGroupItemLst[0].Description;
                tempi.EntityGroupItemLst.RemoveAt(0);

                core.EntityGroupUpdate(tempi);

                var tempii = core.EntityGroupRead(oEG.EntityGroupMUId);
                tempii.EntityGroupItemLst.Add(new EntityItem("Item1", "2nd insert", "3", "created"));
                checkValueB += tempii.EntityGroupItemLst[1].Description;

                core.EntityGroupUpdate(tempii);

                var tempiii = core.EntityGroupRead(oEG.EntityGroupMUId);
                checkValueB += sqlController.UnitTest_EntitiesAllSynced(oEG.EntityGroupMUId);

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test009_Entity_5a_EntityGroupDelete_EntitySearch()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "4isZero";
                string checkValueB;
                EntityGroup oEG;

                //Act
                oEG = core.EntityGroupCreate("EntitySearch", "MyTest");
                List<EntityItem> lst = new List<EntityItem>();
                EntityGroup eG = new EntityGroup(oEG.Id, "Group", "EntitySearch", oEG.EntityGroupMUId, lst, "created", DateTime.Now, DateTime.Now);
                lst.Add(new EntityItem("Item1", "description", "1", "created"));
                lst.Add(new EntityItem("Item2", "description", "2", "created"));
                lst.Add(new EntityItem("Item3", "description", "3", "created"));
                lst.Add(new EntityItem("Item4", "description", "4", "created"));
                core.EntityGroupUpdate(eG);

                var tempi = core.EntityGroupRead(oEG.EntityGroupMUId);
                checkValueB = tempi.EntityGroupItemLst.Count.ToString();

                core.EntityGroupDelete(oEG.EntityGroupMUId);
                var tempii = core.EntityGroupRead(oEG.EntityGroupMUId);

                if (tempii.EntityGroupItemLst.Count == 0)
                    checkValueB += "isZero";

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test009_Entity_5b_EntityGroupDelete_EntitySelect()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "4isZero";
                string checkValueB;
                EntityGroup oEG;

                //Act
                oEG = core.EntityGroupCreate("EntitySelect", "MyTest");
                List<EntityItem> lst = new List<EntityItem>();
                EntityGroup eG = new EntityGroup(oEG.Id, "Group", "EntitySelect", oEG.EntityGroupMUId, lst, "created", DateTime.Now, DateTime.Now);
                lst.Add(new EntityItem("Item1", "description", "1", "created"));
                lst.Add(new EntityItem("Item2", "description", "2", "created"));
                lst.Add(new EntityItem("Item3", "description", "3", "created"));
                lst.Add(new EntityItem("Item4", "description", "4", "created"));
                core.EntityGroupUpdate(eG);

                var tempi = core.EntityGroupRead(oEG.EntityGroupMUId);
                checkValueB = tempi.EntityGroupItemLst.Count.ToString();

                core.EntityGroupDelete(oEG.EntityGroupMUId);
                var tempii = core.EntityGroupRead(oEG.EntityGroupMUId);

                if (tempii.EntityGroupItemLst.Count == 0)
                    checkValueB += "isZero";

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }
        #endregion

        #region - test 010x - case
        [Fact]
        public void Test010_Case_1a_Retrived()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "Retrived";
                string checkValueB;
                int templatId = -1;
                string mUId;
                MainElement main = new MainElement();

                //Act
                main = main.XmlToClass(LoadFil("requestXmlFromClass.txt"));
                templatId = core.TemplateCreate(main);
                main = core.TemplateRead(templatId);
                core.CaseCreate(main, "", siteId1);
                List<string> mUIds = WaitForAvailableDB();

                mUId = mUIds[0];
                sqlController.NotificationCreate("42", mUId, "unit_fetch");

                while (sqlController.UnitTest_FindAllActiveNotifications().Count > 0)
                    Thread.Sleep(100);

                Case_Dto caseDto = core.CaseLookupMUId(mUId);
                checkValueB = caseDto.Stat;

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test010_Case_2a_Completed()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "Completed";
                string checkValueB;
                string mUId;

                //Act
                mUId = CaseCreate();
                CaseComplet(mUId, null);

                Case_Dto caseDto = core.CaseLookupMUId(mUId);
                checkValueB = caseDto.Stat;

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test010_Case_2b_Reversed_Completed()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "Completed";
                string checkValueB;
                string mUId;

                //Act
                mUId = CaseCreateReversed();
                CaseComplet(mUId, "");

                Case_Dto caseDto = core.CaseLookupMUId(mUId);
                checkValueB = caseDto.Stat;

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test010_Case_3a_Many_Completed()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                int checkValueA = 0;
                int checkValueB;
                Case_Dto caseDto;
                string mUId1; string mUId2; string mUId3; string mUId4;
                int count = 0;

                //Act
                mUId1 = CaseCreate();
                mUId2 = CaseCreate();
                mUId3 = CaseCreate();
                CaseComplet(mUId3, null);
                mUId4 = CaseCreate();
                CaseComplet(mUId4, null);
                CaseComplet(mUId2, null);
                CaseComplet(mUId1, null);

                caseDto = core.CaseLookupMUId(mUId1);
                if (caseDto.Stat != "Completed")
                    count++;

                caseDto = core.CaseLookupMUId(mUId2);
                if (caseDto.Stat != "Completed")
                    count++;

                caseDto = core.CaseLookupMUId(mUId3);
                if (caseDto.Stat != "Completed")
                    count++;

                caseDto = core.CaseLookupMUId(mUId4);
                if (caseDto.Stat != "Completed")
                    count++;

                checkValueB = count;

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        //[Fact]
        //public void T085_CaseReversedMany_Completed()
        //{
        //    if (testContextTravis)
        //        {Assert.Equal(true, true); return;}
        //
        //    lock (_testLock)
        //    {
        //        //Arrange
        //        PrepareForTest();
        //        int checkValueA = 0;
        //        int checkValueB;
        //        Case_Dto caseDto;
        //        string mUId1;
        //        string mUId2;
        //        int count = 0;


        //        //...
        //        //Act
        //        mUId1 = CaseCreate();
        //        mUId2 = CaseCreate();
        //        CaseComplet(mUId2, "");
        //        CaseComplet(mUId1, "");
        //        CaseComplet(mUId1, "1");
        //        CaseComplet(mUId1, "2");
        //        CaseComplet(mUId1, "3");
        //        CaseComplet(mUId2, "1");

        //        caseDto = core.CaseLookupMUId(mUId1);
        //        if (caseDto.Stat != "Completed")
        //            count++;

        //        caseDto = core.CaseLookupMUId(mUId2);
        //        if (caseDto.Stat != "Completed")
        //            count++;

        //        checkValueB = count;


        //        //...
        //        //Assert
        //        Assert.Equal(checkValueA, checkValueB);
        //    }
        //}
        #endregion

        #region - test 011x - interaction cases
        [Fact]
        public void Test011_Interaction_Case_1a_CreatedInTable()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "Passed";
                string checkValueB;

                string xmlStr = LoadFil("xml.txt");
                MainElement main = core.TemplateFromXml(xmlStr);
                int templatId = core.TemplateCreate(main);
                List<int> siteUIds = new List<int>();
                siteUIds.Add(siteId1);
                siteUIds.Add(siteId2);

                //Act
                checkValueB = "" + core.Advanced_InteractionCaseCreate(templatId, "", siteUIds, "", false, null);
                if (checkValueB == "1" || checkValueB == "2")
                    checkValueB = "Passed";

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test011_Interaction_Case_2a_Completed_Connected()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "CompletedCompleted";
                string checkValueB = "";

                string xmlStr = LoadFil("xml.txt");
                MainElement main = core.TemplateFromXml(xmlStr);
                int templatId = core.TemplateCreate(main);
                List<int> siteUIds = new List<int>();
                siteUIds.Add(siteId1);
                siteUIds.Add(siteId2);

                //Act
                int iCaseId = (int)core.Advanced_InteractionCaseCreate(templatId, "", siteUIds, "", true, null);

                WaitForAvailableMicroting(iCaseId);

                InteractionCaseComplet(iCaseId);

                var lst = sqlController.UnitTest_FindAllActiveInteractionCaseLists(iCaseId);

                foreach (var item in lst)
                    checkValueB += item.stat;

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test011_Interaction_Case_2b_Completed_NotConnected()
        {
             lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "CompletedCompleted";
                string checkValueB = "";

                string xmlStr = LoadFil("xml.txt");
                MainElement main = core.TemplateFromXml(xmlStr);
                int templatId = core.TemplateCreate(main);
                List<int> siteUIds = new List<int>();
                siteUIds.Add(siteId1);
                siteUIds.Add(siteId2);

                //Act
                int iCaseId = (int)core.Advanced_InteractionCaseCreate(templatId, t.GetMethodName(), siteUIds, "", false, null);

                WaitForAvailableMicroting(iCaseId);
                InteractionCaseComplet(iCaseId);

                var lst = sqlController.UnitTest_FindAllActiveInteractionCaseLists(iCaseId);
                foreach (var item in lst)
                    checkValueB += item.stat;

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test011_Interaction_Case_2c_CompletedWithReplacements()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "CompletedCompleted";
                string checkValueB = "";

                string xmlStr = LoadFil("xml.txt");
                MainElement main = core.TemplateFromXml(xmlStr);
                int templatId = core.TemplateCreate(main);
                List<int> siteUIds = new List<int>();
                siteUIds.Add(siteId1);
                siteUIds.Add(siteId2);

                //Act
                List<string> temp = new List<string>();
                temp.Add("pre_text1==post_text1");
                temp.Add("pre_text2==post_text2");
                temp.Add("Title::Test");
                temp.Add("Info::info TEXT added to eForm");
                temp.Add("Expire::" + DateTime.Now.AddDays(10).ToString());
                int iCaseId = (int)core.Advanced_InteractionCaseCreate(templatId, "", siteUIds, "", false, temp);

                WaitForAvailableMicroting(iCaseId);
                InteractionCaseComplet(iCaseId);

                var lst = sqlController.UnitTest_FindAllActiveInteractionCaseLists(iCaseId);
                foreach (var item in lst)
                    checkValueB += item.stat;

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test011_Interaction_Case_2d_ReplacementsFailedDateTime()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "failed to sync";
                string checkValueB = "";

                string xmlStr = LoadFil("xml.txt");
                MainElement main = core.TemplateFromXml(xmlStr);
                int templatId = core.TemplateCreate(main);
                List<int> siteUIds = new List<int>();
                siteUIds.Add(siteId1);
                siteUIds.Add(siteId2);

                //Act
                List<string> temp = new List<string>();
                temp.Add("Expire::" + "TEXT THAT IS NOT A DATETIME");
                int iCaseId = (int)core.Advanced_InteractionCaseCreate(templatId, t.GetMethodName(), siteUIds, "", false, temp);

                WaitForAvailableMicroting(iCaseId);
                var lst = sqlController.UnitTest_FindAllActiveInteractionCaseLists(iCaseId);

                var cas = sqlController.UnitTest_FindInteractionCase(iCaseId);
                checkValueB = cas.workflow_state;

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test011_Interaction_Case_3a_DeletedSDK()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "DeletedDeleted";
                string checkValueB = "";

                string xmlStr = LoadFil("xml.txt");
                MainElement main = core.TemplateFromXml(xmlStr);
                int templatId = core.TemplateCreate(main);
                List<int> siteUIds = new List<int>();
                siteUIds.Add(siteId1);
                siteUIds.Add(siteId2);


                //Act
                int iCaseId = (int)core.Advanced_InteractionCaseCreate(templatId, t.GetMethodName(), siteUIds, "", false, null);

                WaitForAvailableMicroting(iCaseId);
                var lst = sqlController.UnitTest_FindAllActiveInteractionCaseLists(iCaseId);

                utCore.CaseDelete(lst[1].microting_uid);
                sqlController.NotificationCreate(DateTime.Now.ToLongTimeString(), lst[0].microting_uid, "unit_fetch");

                while (sqlController.UnitTest_FindAllActiveNotifications().Count > 0)
                    Thread.Sleep(100);

                utCore.CaseDelete(lst[0].microting_uid);
                var lst2 = sqlController.UnitTest_FindAllActiveInteractionCaseLists(iCaseId);

                foreach (var item in lst2)
                    checkValueB += item.stat;

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test011_Interaction_Case_3b_DeletedInteraction()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "DeletedDeleted";
                string checkValueB = "";

                string xmlStr = LoadFil("xml.txt");
                MainElement main = core.TemplateFromXml(xmlStr);
                int templatId = core.TemplateCreate(main);
                List<int> siteUIds = new List<int>();
                siteUIds.Add(siteId1);
                siteUIds.Add(siteId2);

                //Act
                int iCaseId = (int)core.Advanced_InteractionCaseCreate(templatId, t.GetMethodName(), siteUIds, "", false, null);

                WaitForAvailableMicroting(iCaseId);
                var lst = sqlController.UnitTest_FindAllActiveInteractionCaseLists(iCaseId);

                sqlController.NotificationCreate(DateTime.Now.ToLongTimeString(), lst[0].microting_uid, "unit_fetch");
                while (sqlController.UnitTest_FindAllActiveNotifications().Count > 0)
                    Thread.Sleep(100);

                core.Advanced_InteractionCaseDelete(iCaseId);
                while (sqlController.UnitTest_FindAllActiveInteraction().Count > 0)
                    Thread.Sleep(100);

                var lst2 = sqlController.UnitTest_FindAllActiveInteractionCaseLists(iCaseId);
                foreach (var item in lst2)
                    checkValueB += item.stat;

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        [Fact]
        public void Test011_Interaction_Case_4a__Multi_Completed()
        {
            lock (_lockTest)
            {
                //Arrange
                TestPrepare(t.GetMethodName());
                string checkValueA = "CompletedCompletedCompletedCompletedCompletedCompleted";
                string checkValueB = "";

                string xmlStr = LoadFil("xml.txt");
                MainElement main = core.TemplateFromXml(xmlStr);
                int templatId = core.TemplateCreate(main);
                List<int> siteUIds = new List<int>();
                siteUIds.Add(siteId1);
                siteUIds.Add(siteId2);

                //Act
                int iCaseId1 = (int)core.Advanced_InteractionCaseCreate(templatId, t.GetMethodName() + " case1", siteUIds, "", false, null);
                int iCaseId2 = (int)core.Advanced_InteractionCaseCreate(templatId, t.GetMethodName() + " case2", siteUIds, "", false, null);
                int iCaseId3 = (int)core.Advanced_InteractionCaseCreate(templatId, t.GetMethodName() + " case3", siteUIds, "", false, null);

                WaitForAvailableMicroting(iCaseId1);
                WaitForAvailableMicroting(iCaseId2);
                WaitForAvailableMicroting(iCaseId3);

                InteractionCaseComplet(iCaseId2);
                InteractionCaseComplet(iCaseId3);
                InteractionCaseComplet(iCaseId1);

                var lst = sqlController.UnitTest_FindAllActiveInteractionCaseLists(iCaseId1);
                lst.AddRange(sqlController.UnitTest_FindAllActiveInteractionCaseLists(iCaseId2));
                lst.AddRange(sqlController.UnitTest_FindAllActiveInteractionCaseLists(iCaseId3));

                foreach (var item in lst)
                    checkValueB += item.stat;

                //Assert
                TestTeardown();
                Assert.Equal(checkValueA, checkValueB);
            }
        }

        //[Fact]
        //public void T095_Interaction_Case_Multi_Completed_StressTest()
        //{
        //    if (testContextTravis)
        //      {Assert.Equal(true, true); return;}
        //
        //    lock (_testLock)
        //    {
        //        //Arrange
        //        PrepareForTest(t.GetMethodName());
        //        string checkValueA = "Passed, if no expection";
        //        string checkValueB = "Passed, if no expection";

        //        string xmlStr = LoadFil("xml.txt");
        //        MainElement main = core.TemplateFromXml(xmlStr);
        //        int templatId = core.TemplateCreate(main);
        //        List<int> siteUIds = new List<int>();
        //        siteUIds.Add(siteId);
        //        siteUIds.Add(siteId2);
        //        siteUIds.Add(3888);
        //        siteUIds.Add(3883);
        //        //siteUIds.Add(3878);
        //        //siteUIds.Add(3873);
        //        //siteUIds.Add(3863);
        //        //siteUIds.Add(3858);


        //        //...
        //        //Act
        //        int iCaseId1 = core.Advanced_InteractionCaseCreate(templatId, "", siteUIds, "", false, "");
        //        int iCaseId2 = core.Advanced_InteractionCaseCreate(templatId, "", siteUIds, "", false, "");
        //        int iCaseId3 = core.Advanced_InteractionCaseCreate(templatId, "", siteUIds, "", false, "");
        //        int iCaseId4 = core.Advanced_InteractionCaseCreate(templatId, "", siteUIds, "", false, "");

        //        WaitForAvailableMicroting(iCaseId1);
        //        WaitForAvailableMicroting(iCaseId2);
        //        WaitForAvailableMicroting(iCaseId3);
        //        WaitForAvailableMicroting(iCaseId4);

        //        InteractionCaseComplet(iCaseId2);
        //        InteractionCaseComplet(iCaseId4);
        //        InteractionCaseComplet(iCaseId3);
        //        InteractionCaseComplet(iCaseId1);

        //        var lst = sqlController.UnitTest_FindAllActiveInteractionCaseLists(iCaseId1);
        //        lst.AddRange(sqlController.UnitTest_FindAllActiveInteractionCaseLists(iCaseId2));
        //        lst.AddRange(sqlController.UnitTest_FindAllActiveInteractionCaseLists(iCaseId3));
        //        lst.AddRange(sqlController.UnitTest_FindAllActiveInteractionCaseLists(iCaseId4));

        //        foreach (var item in lst)
        //        {
        //            if (item.stat != "Completed")
        //                throw new Exception("InteractionCase not 'Completed'");
        //        }


        //        //...
        //        //Assert
        //        Assert.Equal(checkValueA, checkValueB);
        //    }
        //}
        #endregion



        #region private
        private List<string> WaitForAvailableDB()
        {
            try
            {
                for (int i = 0; i < 100; i++)
                {
                    List<string> lstMUId = sqlController.UnitTest_FindAllActiveCases();

                    if (lstMUId.Count == 1)
                    {
                        return lstMUId;
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                throw new Exception("WaitForAvailableDB failed. Due to failed 100 attempts");
            }
            catch (Exception ex)
            {
                throw new Exception("WaitForAvailableDB failed", ex);
            }
        }

        private bool WaitForAvailableMicroting(string microtingUId)
        {
            if (useLiveData)
            { 
                try
                {
                    for (int i = 0; i < 100; i++)
                    {
                        string responseStr = communicator.CheckStatus(microtingUId, siteId1);

                        if (responseStr.Contains("<Response><Value type=\"success\">"))
                        {
                            responseStr = communicator.Delete(microtingUId, siteId1);
                            if (responseStr.Contains("<Response><Value type=\"success\">"))
                                return true;
                            else
                                return false;
                        }
                        else
                        {
                            Thread.Sleep(300);
                        }
                    }
                    throw new Exception("WaitForAvailableMicroting failed. Due to failed 25 attempts");
                }
                catch (Exception ex)
                {
                    throw new Exception("WaitForAvailableMicroting failed", ex);
                }
            }
            return true;
        }

        private bool WaitForAvailableMicroting(int interactionCaseId)
        {
            try
            {
                string lastReply = "";

                for (int i = 0; i < 125; i++)
                {
                    var lst = sqlController.UnitTest_FindAllActiveInteractionCaseLists(interactionCaseId);
                    var cas = sqlController.UnitTest_FindInteractionCase(interactionCaseId);

                    if (cas.workflow_state == "failed to sync")
                        return true;

                    int missingCount = 0;

                    foreach (var item in lst)
                    {
                        if (string.IsNullOrEmpty(item.microting_uid))
                            missingCount++;
                    }

                    if (missingCount == 0)
                    {
                        lastReply = "";

                        foreach (var item in lst)
                        {
                            string reply = core.CaseCheck(item.microting_uid);

                            if (!reply.Contains("success"))
                                missingCount++;

                            lastReply += reply + " // ";
                        }

                        if (missingCount == 0)
                            return true;
                    }

                    Thread.Sleep(250 + 12 * i);
                }
                core.log.LogCritical("Not Specified", "TraceMsg:'" + lastReply.Trim() + "'");
                throw new Exception("WaitForAvailableMicroting failed. Due to failed 125 attempts (1+ min)");
            }
            catch (Exception ex)
            {
                throw new Exception("WaitForAvailableMicroting failed", ex);
            }
        }

        private string ClearXml(string inputXmlString)
        {
            inputXmlString = t.LocateReplaceAll(inputXmlString, "<StartDate>", "</StartDate>", "xxx");
            inputXmlString = t.LocateReplaceAll(inputXmlString, "<EndDate>", "</EndDate>", "xxx");
            inputXmlString = t.LocateReplaceAll(inputXmlString, "<Language>", "</Language>", "xxx");
            inputXmlString = t.LocateReplaceAll(inputXmlString, "<Id>", "</Id>", "xxx");
            inputXmlString = t.LocateReplaceAll(inputXmlString, "<DefaultValue>", "</DefaultValue>", "xxx");
            inputXmlString = t.LocateReplaceAll(inputXmlString, "<MaxValue>", "</MaxValue>", "xxx");
            inputXmlString = t.LocateReplaceAll(inputXmlString, "<MinValue>", "</MinValue>", "xxx");

            return inputXmlString;
        }

        private string CaseCreate()
        {
            MainElement main = new MainElement();
            main = main.XmlToClass(LoadFil("requestXmlFromClass.txt"));
            int templatId = core.TemplateCreate(main);
            main = core.TemplateRead(templatId);

            return core.CaseCreate(main, "", siteId1);
        }

        private string CaseCreateReversed()
        {
            List<int> siteLst = new List<int> { siteId1 };
            MainElement main = new MainElement();
            main = main.XmlToClass(LoadFil("requestXmlFromClass.txt"));
            main.Repeated = 0;
            int templatId = core.TemplateCreate(main);
            main = core.TemplateRead(templatId);

            return core.CaseCreate(main, "", siteLst, "")[0];
        }

        private void CaseComplet(string microtingUId, string checkUId)
        {
            WaitForAvailableMicroting(microtingUId);

            sqlController.NotificationCreate(DateTime.Now.ToLongTimeString(), microtingUId, "unit_fetch");

            while (sqlController.UnitTest_FindAllActiveNotifications().Count > 0)
                Thread.Sleep(100);

            sqlController.NotificationCreate(DateTime.Now.ToLongTimeString(), microtingUId, "check_status");

            while (sqlController.UnitTest_FindAllActiveNotifications().Count > 0)
                Thread.Sleep(100);

            if (checkUId != null)
                sqlController.CaseCreate(2, siteId1, microtingUId, checkUId, "", "", DateTime.Now);

            utCore.CaseComplet(microtingUId, checkUId);

            if (checkUId != null)
                communicator.Delete(microtingUId, siteId1);
        }

        private void InteractionCaseComplet(int interactionCaseId)
        {
            var lst = sqlController.UnitTest_FindAllActiveInteractionCaseLists(interactionCaseId);

            foreach (var item in lst)
            {
                CaseComplet(item.microting_uid, null);
            }
        }

        private string LoadFil(string path)
        {
            try
            {
                lock (_lockFil)
                {
                    string str = "";
                    using (StreamReader sr = new StreamReader(path, true))
                    {
                        str = sr.ReadToEnd();
                    }
                    return str;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load fil", ex);
            }
        }
        #endregion

        #region events
        public void EventCaseCreated(object sender, EventArgs args)
        {
            ////DOSOMETHING: changed to fit your wishes and needs 
            //Case_Dto temp = (Case_Dto)sender;
            //int siteId = temp.SiteId;
            //string caseType = temp.CaseType;
            //string caseUid = temp.CaseUId;
            //string mUId = temp.MicrotingUId;
            //string checkUId = temp.CheckUId;
        }

        public void EventCaseRetrived(object sender, EventArgs args)
        {
            ////DOSOMETHING: changed to fit your wishes and needs 
            //Case_Dto temp = (Case_Dto)sender;
            //int siteId = temp.SiteId;
            //string caseType = temp.CaseType;
            //string caseUid = temp.CaseUId;
            //string mUId = temp.MicrotingUId;
            //string checkUId = temp.CheckUId;
        }

        public void EventCaseCompleted(object sender, EventArgs args)
        {
            ////DOSOMETHING: changed to fit your wishes and needs
            //Case_Dto temp = (Case_Dto)sender;
            //int siteId = temp.SiteId;
            //string caseType = temp.CaseType;
            //string caseUid = temp.CaseUId;
            //string mUId = temp.MicrotingUId;
            //string checkUId = temp.CheckUId;
        }

        public void EventCaseDeleted(object sender, EventArgs args)
        {
            ////DOSOMETHING: changed to fit your wishes and needs
            //Case_Dto temp = (Case_Dto)sender;
            //int siteId = temp.SiteId;
            //string caseType = temp.CaseType;
            //string caseUid = temp.CaseUId;
            //string mUId = temp.MicrotingUId;
            //string checkUId = temp.CheckUId;
        }

        public void EventFileDownloaded(object sender, EventArgs args)
        {
            ////DOSOMETHING: changed to fit your wishes and needs 
            //File_Dto temp = (File_Dto)sender;
            //int siteId = temp.SiteId;
            //string caseType = temp.CaseType;
            //string caseUid = temp.CaseUId;
            //string mUId = temp.MicrotingUId;
            //string checkUId = temp.CheckUId;
            //string fileLocation = temp.FileLocation;
        }

        public void EventSiteActivated(object sender, EventArgs args)
        {
            ////DOSOMETHING: changed to fit your wishes and needs 
            //int siteId = int.Parse(sender.ToString());
        }

        public void EventNotificationNotFound(object sender, EventArgs args)
        {

        }

        public void EventException(object sender, EventArgs args)
        {
            lock (_lockFil)
            {
                File.AppendAllText(@"log\\exception.txt", sender + Environment.NewLine);
            }

            throw (Exception)sender;
        }
        #endregion
    }

    #region dummy class
    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<TestContext>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
    #endregion
}