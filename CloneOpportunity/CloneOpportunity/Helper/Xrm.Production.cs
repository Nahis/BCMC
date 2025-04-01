using System;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Xrm
{
    public enum OpportunityState
    {
        Active = 0,
        Won = 1,
        Lost = 2
    }

    public enum New_roompatternState
    {
        Active = 0,
        Inactive = 1
    }

    [DataContract]
    [EntityLogicalName("opportunity")]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "9.0.0.9369")]
    public partial class Opportunity : Entity
    {
        public const string EntityLogicalName = "opportunity";
        public const int EntityTypeCode = 3;

        public Opportunity() : base(EntityLogicalName)
        {
        }

        [AttributeLogicalName("opportunityid")]
        public Guid? OpportunityId
        {
            get { return GetAttributeValue<Guid?>("opportunityid"); }
            set { SetAttributeValue("opportunityid", value); }
        }

        [AttributeLogicalName("bcmc_reportactualized")]
        public bool? bcmc_ReportActualized
        {
            get { return GetAttributeValue<bool?>("bcmc_reportactualized"); }
            set { SetAttributeValue("bcmc_reportactualized", value); }
        }

        [AttributeLogicalName("bcmc_actualizeddayspriorevent")]
        public int? bcmc_ActualizedDaysPriorEvent
        {
            get { return GetAttributeValue<int?>("bcmc_actualizeddayspriorevent"); }
            set { SetAttributeValue("bcmc_actualizeddayspriorevent", value); }
        }

        [AttributeLogicalName("bcmc_actualizeddayspostevent")]
        public int? bcmc_ActualizedDaysPostEvent
        {
            get { return GetAttributeValue<int?>("bcmc_actualizeddayspostevent"); }
            set { SetAttributeValue("bcmc_actualizeddayspostevent", value); }
        }

        [AttributeLogicalName("new_arrivaldate")]
        public DateTime? New_arrivaldate
        {
            get { return GetAttributeValue<DateTime?>("new_arrivaldate"); }
            set { SetAttributeValue("new_arrivaldate", value); }
        }

        [AttributeLogicalName("new_departuredate")]
        public DateTime? New_departuredate
        {
            get { return GetAttributeValue<DateTime?>("new_departuredate"); }
            set { SetAttributeValue("new_departuredate", value); }
        }

        [AttributeLogicalName("new_hotelroomnights")]
        public int? New_HotelRoomNights
        {
            get { return GetAttributeValue<int?>("new_hotelroomnights"); }
            set { SetAttributeValue("new_hotelroomnights", value); }
        }

        [AttributeLogicalName("new_peakhotelroomnights")]
        public int? New_PeakHotelRoomNights
        {
            get { return GetAttributeValue<int?>("new_peakhotelroomnights"); }
            set { SetAttributeValue("new_peakhotelroomnights", value); }
        }

        [AttributeLogicalName("bcmc_actualizedentrycomplete")]
        public bool? bcmc_ActualizedEntryComplete
        {
            get { return GetAttributeValue<bool?>("bcmc_actualizedentrycomplete"); }
            set { SetAttributeValue("bcmc_actualizedentrycomplete", value); }
        }

        [AttributeLogicalName("statecode")]
        public OpportunityState? StateCode
        {
            get { return (OpportunityState?)GetAttributeValue<OptionSetValue>("statecode")?.Value; }
            set { SetAttributeValue("statecode", value.HasValue ? new OptionSetValue((int)value.Value) : null); }
        }

        [AttributeLogicalName("bcmc_eventyear")]
        public string BCMC_EventYear
        {
            get { return GetAttributeValue<string>("bcmc_eventyear"); }
            set { SetAttributeValue("bcmc_eventyear", value); }
        }

        [AttributeLogicalName("statuscode")]
        public OptionSetValue StatusCode
        {
            get { return GetAttributeValue<OptionSetValue>("statuscode"); }
            set { SetAttributeValue("statuscode", value); }
        }

        [AttributeLogicalName("bcmc_actualhotelroomnights")]
        public int? bcmc_ActualHotelRoomNights
        {
            get { return GetAttributeValue<int?>("bcmc_actualhotelroomnights"); }
            set { SetAttributeValue("bcmc_actualhotelroomnights", value); }
        }

        [AttributeLogicalName("bcmc_actualpeakhotelroomnights")]
        public int? bcmc_ActualPeakHotelRoomNights
        {
            get { return GetAttributeValue<int?>("bcmc_actualpeakhotelroomnights"); }
            set { SetAttributeValue("bcmc_actualpeakhotelroomnights", value); }
        }
    }

    [DataContract]
    [EntityLogicalName("new_eventsite")]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "9.0.0.9369")]
    public partial class New_EventSite : Entity
    {
        public const string EntityLogicalName = "new_eventsite";
        public const int EntityTypeCode = 10001;

        public New_EventSite() : base(EntityLogicalName)
        {
        }

        [AttributeLogicalName("new_eventsiteid")]
        public Guid? New_EventSiteId
        {
            get { return GetAttributeValue<Guid?>("new_eventsiteid"); }
            set { SetAttributeValue("new_eventsiteid", value); }
        }

        [AttributeLogicalName("new_eventid")]
        public EntityReference New_EventId
        {
            get { return GetAttributeValue<EntityReference>("new_eventid"); }
            set { SetAttributeValue("new_eventid", value); }
        }

        [AttributeLogicalName("new_history")]
        public string New_History
        {
            get { return GetAttributeValue<string>("new_history"); }
            set { SetAttributeValue("new_history", value); }
        }

        [AttributeLogicalName("new_notes")]
        public string New_Notes
        {
            get { return GetAttributeValue<string>("new_notes"); }
            set { SetAttributeValue("new_notes", value); }
        }

        [AttributeLogicalName("new_cityid")]
        public EntityReference New_CityId
        {
            get { return GetAttributeValue<EntityReference>("new_cityid"); }
            set { SetAttributeValue("new_cityid", value); }
        }

        [AttributeLogicalName("bcmc_newyear")]
        public string BCMC_NewYear
        {
            get { return GetAttributeValue<string>("bcmc_newyear"); }
            set { SetAttributeValue("bcmc_newyear", value); }
        }

        [AttributeLogicalName("bcmc_actualblock")]
        public int? bcmc_ActualBlock
        {
            get { return GetAttributeValue<int?>("bcmc_actualblock"); }
            set { SetAttributeValue("bcmc_actualblock", value); }
        }

        [AttributeLogicalName("bcmc_totalroomnights")]
        public int? BCMC_TotalRoomNights
        {
            get { return GetAttributeValue<int?>("bcmc_totalroomnights"); }
            set { SetAttributeValue("bcmc_totalroomnights", value); }
        }

        [AttributeLogicalName("bcmc_peakblockactualized")]
        public int? BCMC_PeakBlockActualized
        {
            get { return GetAttributeValue<int?>("bcmc_peakblockactualized"); }
            set { SetAttributeValue("bcmc_peakblockactualized", value); }
        }
    }

    [DataContract(Namespace = "http://schemas.microsoft.com/xrm/2011/Contracts")]
    [EntityLogicalName("new_roompattern")]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "9.0.0.9369")]
    public partial class New_roompattern : Entity
    {
        public const string EntityLogicalName = "new_roompattern";
        public const int EntityTypeCode = 10002;

        public New_roompattern() : base(EntityLogicalName)
        {
        }

        [DataMember]
        [AttributeLogicalName("new_roompatternid")]
        public Guid? New_roompatternId
        {
            get { return GetAttributeValue<Guid?>("new_roompatternid"); }
            set { SetAttributeValue("new_roompatternid", value); }
        }

        [DataMember]
        [AttributeLogicalName("new_eventid")]
        public EntityReference New_EventId
        {
            get { return GetAttributeValue<EntityReference>("new_eventid"); }
            set { SetAttributeValue("new_eventid", value); }
        }

        [DataMember]
        [AttributeLogicalName("new_daynumber")]
        public int? New_DayNumber
        {
            get { return GetAttributeValue<int?>("new_daynumber"); }
            set { SetAttributeValue("new_daynumber", value); }
        }

        [DataMember]
        [AttributeLogicalName("new_name")]
        public string New_name
        {
            get { return GetAttributeValue<string>("new_name"); }
            set { SetAttributeValue("new_name", value); }
        }

        [DataMember]
        [AttributeLogicalName("bcmc_date")]
        public DateTime? Bcmc_Date
        {
            get { return GetAttributeValue<DateTime?>("bcmc_date"); }
            set { SetAttributeValue("bcmc_date", value); }
        }

        [DataMember]
        [AttributeLogicalName("bcmc_originalpercentofpeak")]
        public int? Bcmc_OriginalpercentofPeak
        {
            get { return GetAttributeValue<int?>("bcmc_originalpercentofpeak"); }
            set { SetAttributeValue("bcmc_originalpercentofpeak", value); }
        }

        [DataMember]
        [AttributeLogicalName("bcmc_originalroomblock")]
        public int? Bcmc_OriginalRoomBlock
        {
            get { return GetAttributeValue<int?>("bcmc_originalroomblock"); }
            set { SetAttributeValue("bcmc_originalroomblock", value); }
        }

        [DataMember]
        [AttributeLogicalName("new_percentofpeak")]
        public int? New_PercentofPeak
        {
            get { return GetAttributeValue<int?>("new_percentofpeak"); }
            set { SetAttributeValue("new_percentofpeak", value); }
        }

        [DataMember]
        [AttributeLogicalName("new_roomblock")]
        public int? New_RoomBlock
        {
            get { return GetAttributeValue<int?>("new_roomblock"); }
            set { SetAttributeValue("new_roomblock", value); }
        }

        [DataMember]
        [AttributeLogicalName("bcmc_actualblock")]
        public int? bcmc_ActualBlock
        {
            get { return GetAttributeValue<int?>("bcmc_actualblock"); }
            set { SetAttributeValue("bcmc_actualblock", value); }
        }

        [DataMember]
        [AttributeLogicalName("bcmc_actualpercentofpeak")]
        public int? bcmc_ActualPercentOfPeak
        {
            get { return GetAttributeValue<int?>("bcmc_actualpercentofpeak"); }
            set { SetAttributeValue("bcmc_actualpercentofpeak", value); }
        }

        [DataMember]
        [AttributeLogicalName("bcmc_user")]
        public string bcmc_User
        {
            get { return GetAttributeValue<string>("bcmc_user"); }
            set { SetAttributeValue("bcmc_user", value); }
        }

        [DataMember]
        [AttributeLogicalName("bcmc_statustype")]
        public int? Bcmc_statustype
        {
            get { return GetAttributeValue<int?>("bcmc_statustype"); }
            set { SetAttributeValue("bcmc_statustype", value); }
        }

        [DataMember]
        [AttributeLogicalName("statecode")]
        public New_roompatternState? StateCode
        {
            get { return (New_roompatternState?)GetAttributeValue<OptionSetValue>("statecode")?.Value; }
            set { SetAttributeValue("statecode", value.HasValue ? new OptionSetValue((int)value.Value) : null); }
        }
    }

    [DataContract]
    [EntityLogicalName("competitor")]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "9.0.0.9369")]
    public partial class Competitor : Entity
    {
        public const string EntityLogicalName = "competitor";
        public const int EntityTypeCode = 123;

        public Competitor() : base(EntityLogicalName)
        {
        }

        [AttributeLogicalName("competitorid")]
        public Guid? CompetitorId
        {
            get { return GetAttributeValue<Guid?>("competitorid"); }
            set { SetAttributeValue("competitorid", value); }
        }

        [AttributeLogicalName("name")]
        public string Name
        {
            get { return GetAttributeValue<string>("name"); }
            set { SetAttributeValue("name", value); }
        }

        [AttributeLogicalName("statecode")]
        public OptionSetValue StateCode
        {
            get { return GetAttributeValue<OptionSetValue>("statecode"); }
            set { SetAttributeValue("statecode", value); }
        }
    }
}