using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.ComponentModel;

namespace RCommon
{
    //This event is raised to pass a message to the container's project manager.
    public class RunButtonStatusArgs : EventArgs
    {
        private bool boolRunButtonStatus;

        public RunButtonStatusArgs(bool status)
        {
            this.boolRunButtonStatus = status;
        }
        
        public bool Status 
        {
            get { return(boolRunButtonStatus); }
        }
    }
}