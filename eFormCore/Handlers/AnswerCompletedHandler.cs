/*
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
using System.Threading.Tasks;
using Microting.eForm.Infrastructure;
using Microting.eForm.Infrastructure.Constants;
using Microting.eForm.Messages;
using Rebus.Handlers;

namespace Microting.eForm.Handlers
{
    public class AnswerCompletedHandler : IHandleMessages<AnswerCompleted>
    {
        private readonly SqlController sqlController;
        //private readonly Communicator communicator;
        private readonly Log log;
        //private readonly Core core;
        Tools t = new Tools();

        public AnswerCompletedHandler(SqlController sqlController, Log log)
        {
            this.sqlController = sqlController;
            //this.communicator = communicator;
            this.log = log;
            //this.core = core;
        }

        
        public async Task Handle(AnswerCompleted message)
        {
            try
            {
                await GetAnswer(message.MicrotringUUID);
                await sqlController.NotificationUpdate(message.NotificationUId, 
                    message.MicrotringUUID,
                    Constants.WorkflowStates.Processed, 
                    "", 
                    "");
            }
            catch (Exception ex)
            {
                await sqlController.NotificationUpdate(message.NotificationUId, 
                    message.MicrotringUUID, 
                    Constants.WorkflowStates.NotFound, 
                    ex.Message, 
                    ex.StackTrace.ToString());
            }
            
            
        }

        private async Task GetAnswer(int microtingUid)
        {
            await Task.Run(() => { });
        }
    }
}