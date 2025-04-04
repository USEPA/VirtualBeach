using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Net;
using System.Collections.Generic;

namespace Prediction
{
    public static class EnddatCommon
    {
        //If no specific date was indicated, use Today.
        public static string FormatEnddatQuery(String EnddatURL, string Timestamp, string Timezone, bool UseTimestamp = true)
        {
            return FormatEnddatQuery(EnddatURL, DateTime.Today, Timestamp, Timezone, UseTimestamp);
        }

        public static string FormatEnddatQuery(String EnddatURL, DateTime dateToImport, string Timestamp, string Timezone, bool UseTimestamp = true)
        {
            Regex reShapefile = new Regex("&?shapefile=[^&]+");
            Regex reShapefileFeature = new Regex("&?shapefileFeature=[^&]+");
            Regex reGeoDataPortalID = new Regex("&?gdpId=[^&]+");
            Regex reGeoDataPortalVariable = new Regex("&?GDP=[^&]+");

            if (reShapefile.Match(EnddatURL).Success)
            {
                string strShapefileFeature = "ID", strShapefile, strGeoDataPortalID, strStatistic = "MEAN", strPreamble = "collection:MEAN:", strVariable;
                strShapefile = reShapefile.Match(EnddatURL).Value.Split('=')[1];

                if (reShapefileFeature.Match(EnddatURL).Success) { strShapefileFeature = reShapefileFeature.Match(EnddatURL).Value.Split('=')[1]; }
                //if (strShapefileFeature == "Id") { strShapefileFeature = "ID"; }
                if (reGeoDataPortalVariable.Match(EnddatURL).Success) { strVariable = reGeoDataPortalVariable.Match(EnddatURL).Value.Split('=')[1]; }
                if (reGeoDataPortalID.Match(EnddatURL).Success)
                {
                    strGeoDataPortalID = reGeoDataPortalID.Match(EnddatURL).Value;
                    EnddatURL = reGeoDataPortalID.Replace(EnddatURL, "");
                    string[] chunks = strGeoDataPortalID.Split('=')[1].Split(':');
                    strStatistic = chunks[1];
                    strPreamble = chunks[0] + ":" + chunks[1] + ":";
                }

                DateTime dtStart = dateToImport.Subtract(new TimeSpan(days: 7, hours: 0, minutes: 0, seconds: 0));
                DateTime dtEnd = dateToImport;

                string strStart = dtStart.Year.ToString() + "-" + dtStart.Month.ToString("D2") + "-" + dtStart.Day.ToString("D2");
                string strEnd = dtEnd.Year.ToString() + "-" + dtEnd.Month.ToString("D2") + "-" + dtEnd.Day.ToString("D2");

                string strNewGeoDataPortalID = EnddatCommon.GetGeoDataPortalID(Shapefile: strShapefile, Feature: strShapefileFeature, Start: strStart, End: strEnd, Statistic: strStatistic);
                if (strNewGeoDataPortalID == null) { return ""; }
                EnddatURL = EnddatURL + "&gdpId=" + strPreamble + strNewGeoDataPortalID;
            }

            //Set the timezone for the imported data
            string strTimezone;
            string strTimestamp;

            if (Timezone == "Local time")
            {
                strTimezone = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).ToString().Split(':')[0].Trim();
            }
            else
            {
                strTimezone = Timezone.Split(':')[0].Trim();
            }

            //Prepend a zero if necessary:
            if (strTimezone[0] == '-' && strTimezone.Length == 2)
            {
                strTimezone = "-0" + strTimezone[1];
            }
            else if (strTimezone[0] == '+' && strTimezone.Length == 2)
            {
                strTimezone = "+0" + strTimezone[1];
            }

            if (UseTimestamp)
            {
                strTimestamp = "T" + Timestamp + strTimezone + "00";
                EnddatURL = EnddatURL + "&style=csv&TZ=" + strTimezone + "&datetime=" + strTimestamp;
            }
            else
            {
                EnddatURL = EnddatURL + "&style=csv&latest=TRUE&timeInt=36&TZ=" + strTimezone;
            }

            return EnddatURL;
        }





        public static string GetGeoDataPortalID(string Shapefile, string Feature, string Start, string End, string Statistic)
        {
            string id = "";

            string featureType = Shapefile;
            string featureAttrName = Feature;
            string dataUrl = "dods://cida.usgs.gov/thredds/rfc_qpe/dodsC/qpe/realtime/kmsr/best";
            string datasetId = "1-hour_Quantitative_Precip_Estimate_surface_1_Hour_Accumulation";
            string endDate = End;
            string startDate = Start;
            string statistic = Statistic;

            string urlToPost = "http://cida.usgs.gov/gdp/process/WebProcessingService";

            string wpsExecute = String.Concat(new string[] {@"<?xml version=""1.0"" encoding=""UTF-8""?>",
@"<wps:Execute xmlns:wps=""http://www.opengis.net/wps/1.0.0"" xmlns:ows=""http://www.opengis.net/ows/1.1"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" service=""WPS"" version=""1.0.0"" xsi:schemaLocation=""http://www.opengis.net/wps/1.0.0 http://schemas.opengis.net/wps/1.0.0/wpsExecute_request.xsd"">",
@"	<ows:Identifier>gov.usgs.cida.gdp.wps.algorithm.FeatureWeightedGridStatisticsAlgorithm</ows:Identifier>",
@"	<wps:DataInputs>",
@"		<wps:Input>",
@"			<ows:Identifier>FEATURE_COLLECTION</ows:Identifier>",
@"			<wps:Reference xlink:href=""http://cida-eros-enddatdev.er.usgs.gov:8080/beaches-geoserver/wfs"">",
@"				<wps:Body>",
@"					<wfs:GetFeature xmlns:gml=""http://www.opengis.net/gml"" xmlns:ogc=""http://www.opengis.net/ogc"" xmlns:wfs=""http://www.opengis.net/wfs"" outputFormat=""text/xml; subtype=gml/3.1.1"" service=""WFS"" version=""1.1.0"" xsi:schemaLocation=""http://www.opengis.net/wfs http://cida-eros-enddatdev.er.usgs.gov:8080/beaches-geoserver/schemas/wfs/1.1.0/wfs.xsd"">",
@" 					<wfs:Query typeName=""", featureType, @""">",
"							<wfs:PropertyName>the_geom</wfs:PropertyName>",
"							<wfs:PropertyName>", featureAttrName, "</wfs:PropertyName>",
"						</wfs:Query>",
"					</wfs:GetFeature>",
"				</wps:Body>",
"			</wps:Reference>",
"		</wps:Input>",
"		<wps:Input>",
"			<ows:Identifier>FEATURE_ATTRIBUTE_NAME</ows:Identifier>",
"			<wps:Data>",
"				<wps:LiteralData>", featureAttrName, "</wps:LiteralData>",
"			</wps:Data>",
"		</wps:Input>",
"		<wps:Input>",
"			<ows:Identifier>DATASET_URI</ows:Identifier>",
"			<wps:Data>",
"				<wps:LiteralData>" , dataUrl , "</wps:LiteralData>",
"			</wps:Data>",
"		</wps:Input>",
"		<wps:Input>",
"			<ows:Identifier>DATASET_ID</ows:Identifier>",
"			<wps:Data>",
"				<wps:LiteralData>" , datasetId , "</wps:LiteralData>",
"			</wps:Data>",
"		</wps:Input>",
"		<wps:Input>",
"			<ows:Identifier>TIME_START</ows:Identifier>",
"			<wps:Data>",
"				<wps:LiteralData>" , startDate , "T00:00:00.000Z</wps:LiteralData>", 
"			</wps:Data>",
"		</wps:Input>",
"		<wps:Input>",
"			<ows:Identifier>TIME_END</ows:Identifier>",
"			<wps:Data>",
"				<wps:LiteralData>" , endDate , "T23:59:59.000Z</wps:LiteralData>", 
"			</wps:Data>",
"		</wps:Input>",
"		<wps:Input>",
"			<ows:Identifier>REQUIRE_FULL_COVERAGE</ows:Identifier>",
"			<wps:Data>",
"				<wps:LiteralData>true</wps:LiteralData>",
"			</wps:Data>",
"		</wps:Input>",
"		<wps:Input>",
"			<ows:Identifier>DELIMITER</ows:Identifier>",
"			<wps:Data>",
"				<wps:LiteralData>COMMA</wps:LiteralData>",
"			</wps:Data>",
"		</wps:Input>",
"  	<wps:Input>",
"			<ows:Identifier>STATISTICS</ows:Identifier>",
"			<wps:Data>",
"				<wps:LiteralData>" , statistic , "</wps:LiteralData>",
"			</wps:Data>",
"		</wps:Input>",
"		<wps:Input>",
"			<ows:Identifier>GROUP_BY</ows:Identifier>",
"			<wps:Data>",
"				<wps:LiteralData>STATISTIC</wps:LiteralData>",
"			</wps:Data>",
"		</wps:Input>",
"		<wps:Input>",
"			<ows:Identifier>SUMMARIZE_TIMESTEP</ows:Identifier>",
"			<wps:Data>",
"				<wps:LiteralData>false</wps:LiteralData>",
"			</wps:Data>",
"		</wps:Input>",
"		<wps:Input>",
"			<ows:Identifier>SUMMARIZE_FEATURE_ATTRIBUTE</ows:Identifier>",
"			<wps:Data>",
"				<wps:LiteralData>false</wps:LiteralData>",
"			</wps:Data>",
"		</wps:Input>",
"	</wps:DataInputs>",
"	<wps:ResponseForm>",
@"		<wps:ResponseDocument storeExecuteResponse= ""true"" status=""true"">",
@"			<wps:Output asReference=""true"" mimetype=""text/csv"">",
"				<ows:Identifier>OUTPUT</ows:Identifier>",
"			</wps:Output>",
"		</wps:ResponseDocument>",
"	</wps:ResponseForm>",
"</wps:Execute>"});

            using (WebDownload client = new WebDownload())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/xml";
                string HtmlResult = client.UploadString(urlToPost, wpsExecute);

                XElement xm = XElement.Parse(HtmlResult);
                string strStatusLocation = xm.Attributes().First(x => x.Name == "statusLocation").Value;

                int i = 0;
                bool bComplete = false;
                while (!bComplete)
                {
                    string strStatusXml = client.DownloadString(strStatusLocation);
                    xm = XElement.Parse(strStatusXml);
                    int intSuccessCount = xm.Elements().First(x => x.Name.LocalName == "Status").Elements().Count(x => x.Name.LocalName == "ProcessSucceeded");
                    if (intSuccessCount > 0)
                    {
                        string strHref = xm.Elements().First(x => x.Name.LocalName == "ProcessOutputs").Elements().First(x => x.Name.LocalName == "Output").Elements().First(x => x.Name.LocalName == "Reference").Attribute("href").Value;
                        List<string> chunks = strHref.Split('?').ToList<string>();
                        string idString = chunks.First(x => x.Split('=')[0] == "id");
                        id = idString.Split('=')[1];
                        bComplete = true;
                    }
                    else
                    {
                        i++;
                        if (i == 10)
                        {
                            i = 0;
                            if (System.Windows.Forms.MessageBox.Show("EnDDaT is taking a while to render the GeoDataPortal shapefile. Do you want to comtinue waiting?", "Continue waiting?", MessageBoxButtons.YesNo) == DialogResult.No)
                            {
                                return null;
                            }
                        }
                        System.Threading.Thread.Sleep(2000);
                    }
                }
            }

            return id;
        }
    }
}
