﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBCommon
{
    public class Globals
    {
        public enum messageIntent { LogFileOnly, UserOnly, UserAndLogFile };
        public enum targetSStrip { None, StatusStrip1, StatusStrip2, StatusStrip3, ProgressBar }

        public enum PluginType
        {
            ProjectManager = -1,
            Map = 0,
            Datasheet = 1,
            Modeling = 2,
            Prediction = 3
        }

        public enum DependentVariableTransforms
        {
            none,
            Log10,
            Ln,
            Power
        }
        
        public enum Transforms
        {
            none = 0,
            square = 1,
            sqrroot = 2,
            logbasee = 3,
            inverse = 4,
            logbase10 = 5,
            quarticroot = 6
        }

        public enum Transforms2 
        {   log10, 
            antilog10, 
            ln, 
            antiln, 
            inverse, 
            square, 
            squareroot, 
            quadroot, 
            polynomial, 
            generalexp 
        }

        public enum Operations
        {
            SUM,
            MEAN,
            PROD,
            MAX,
            MIN,
            DIFF
        }

        public enum ColumnAttributes
        {
            Disabled,
            Enabled,
            Hidden,
            Visible
        }

        public enum ProjectType
        {
            COMPLETE,
            MODEL
        }

        public enum listvals { NCOLS, NROWS, DATECOLNAME, RVCOLNAME, BLANK, NDISABLEDROWS, NDISABLEDCOLS, NHIDDENCOLS, NIVS };

        //datatable column data attributes
        public const string MAINEFFECT = "maineffect";
        public const string TRANSFORM = "transform";
        public const string OPERATION = "operation";
        public const string DECOMPOSITION = "decomposition";
        public const string DEPENDENTVAR = "dependentvariable";
        public const string DATETIMESTAMP = "datatimestamp";
        public const string DEPENDENTVARIBLETRANSFORM = "responsevartransform";
        public const string ENABLED = "enabled";
        public const string HIDDEN = "hidden";
        public const string CATEGORICAL = "categorical";
        public const string DEPENDENTVARIBLEDEFINEDTRANSFORM = "responsevardefinedtransform";
    }
}
