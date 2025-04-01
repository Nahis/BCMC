using System;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System.ComponentModel;

namespace Xrm
{
    [DataContract(Name = "opportunity", Namespace = "http://schemas.microsoft.com/xrm/2011/Contracts")]
    [EntityLogicalName("opportunity")]
    public sealed class opportunity : Entity
    {
        public const string EntityLogicalName = "opportunity";
        public const int EntityTypeCode = 3;

        public opportunity() : base(EntityLogicalName) { }

        [AttributeLogicalName("opportunityid")]
        public override System.Guid Id
        {
            get { return base.Id; }
            set { base.Id = value; }
        }

        [AttributeLogicalName("name")]
        public string name
        {
            get { return GetAttributeValue<string>("name"); }
            set { SetAttributeValue("name", value); }
        }

        [AttributeLogicalName("statecode")]
        public System.Nullable<int> statecode
        {
            get { return GetAttributeValue<System.Nullable<int>>("statecode"); }
            set { SetAttributeValue("statecode", value); }
        }

        [AttributeLogicalName("statuscode")]
        public System.Nullable<int> statuscode
        {
            get { return GetAttributeValue<System.Nullable<int>>("statuscode"); }
            set { SetAttributeValue("statuscode", value); }
        }

        [AttributeLogicalName("parentaccountid")]
        public EntityReference parentaccountid
        {
            get { return GetAttributeValue<EntityReference>("parentaccountid"); }
            set { SetAttributeValue("parentaccountid", value); }
        }

        [AttributeLogicalName("parentcontactid")]
        public EntityReference parentcontactid
        {
            get { return GetAttributeValue<EntityReference>("parentcontactid"); }
            set { SetAttributeValue("parentcontactid", value); }
        }

        [AttributeLogicalName("customerid")]
        public EntityReference customerid
        {
            get { return GetAttributeValue<EntityReference>("customerid"); }
            set { SetAttributeValue("customerid", value); }
        }
    }

    [DataContract(Name = "quote", Namespace = "http://schemas.microsoft.com/xrm/2011/Contracts")]
    [EntityLogicalName("quote")]
    public sealed class quote : Entity
    {
        public const string EntityLogicalName = "quote";
        public const int EntityTypeCode = 1084;

        public quote() : base(EntityLogicalName) { }

        [AttributeLogicalName("quoteid")]
        public override System.Guid Id
        {
            get { return base.Id; }
            set { base.Id = value; }
        }

        [AttributeLogicalName("statecode")]
        public System.Nullable<int> statecode
        {
            get { return GetAttributeValue<System.Nullable<int>>("statecode"); }
            set { SetAttributeValue("statecode", value); }
        }

        [AttributeLogicalName("opportunityid")]
        public EntityReference opportunityid
        {
            get { return GetAttributeValue<EntityReference>("opportunityid"); }
            set { SetAttributeValue("opportunityid", value); }
        }
    }

    [DataContract(Name = "invoice", Namespace = "http://schemas.microsoft.com/xrm/2011/Contracts")]
    [EntityLogicalName("invoice")]
    public sealed class invoice : Entity
    {
        public const string EntityLogicalName = "invoice";
        public const int EntityTypeCode = 1090;

        public invoice() : base(EntityLogicalName) { }

        [AttributeLogicalName("invoiceid")]
        public override System.Guid Id
        {
            get { return base.Id; }
            set { base.Id = value; }
        }

        [AttributeLogicalName("statecode")]
        public System.Nullable<int> statecode
        {
            get { return GetAttributeValue<System.Nullable<int>>("statecode"); }
            set { SetAttributeValue("statecode", value); }
        }
    }

    [DataContract(Name = "salesorder", Namespace = "http://schemas.microsoft.com/xrm/2011/Contracts")]
    [EntityLogicalName("salesorder")]
    public sealed class salesorder : Entity
    {
        public const string EntityLogicalName = "salesorder";
        public const int EntityTypeCode = 1088;

        public salesorder() : base(EntityLogicalName) { }

        [AttributeLogicalName("salesorderid")]
        public override System.Guid Id
        {
            get { return base.Id; }
            set { base.Id = value; }
        }

        [AttributeLogicalName("statecode")]
        public System.Nullable<int> statecode
        {
            get { return GetAttributeValue<System.Nullable<int>>("statecode"); }
            set { SetAttributeValue("statecode", value); }
        }
    }

    [DataContract(Name = "serviceappointment", Namespace = "http://schemas.microsoft.com/xrm/2011/Contracts")]
    [EntityLogicalName("serviceappointment")]
    public sealed class serviceappointment : Entity
    {
        public const string EntityLogicalName = "serviceappointment";
        public const int EntityTypeCode = 4214;

        public serviceappointment() : base(EntityLogicalName) { }

        [AttributeLogicalName("activityid")]
        public override System.Guid Id
        {
            get { return base.Id; }
            set { base.Id = value; }
        }

        [AttributeLogicalName("statecode")]
        public System.Nullable<int> statecode
        {
            get { return GetAttributeValue<System.Nullable<int>>("statecode"); }
            set { SetAttributeValue("statecode", value); }
        }
    }

    [DataContract(Name = "appointment", Namespace = "http://schemas.microsoft.com/xrm/2011/Contracts")]
    [EntityLogicalName("appointment")]
    public sealed class appointment : Entity
    {
        public const string EntityLogicalName = "appointment";
        public const int EntityTypeCode = 4201;

        public appointment() : base(EntityLogicalName) { }

        [AttributeLogicalName("activityid")]
        public override System.Guid Id
        {
            get { return base.Id; }
            set { base.Id = value; }
        }

        [AttributeLogicalName("statecode")]
        public System.Nullable<int> statecode
        {
            get { return GetAttributeValue<System.Nullable<int>>("statecode"); }
            set { SetAttributeValue("statecode", value); }
        }
    }

    [DataContract(Name = "customeropportunityrole", Namespace = "http://schemas.microsoft.com/xrm/2011/Contracts")]
    [EntityLogicalName("customeropportunityrole")]
    public sealed class customeropportunityrole : Entity
    {
        public const string EntityLogicalName = "customeropportunityrole";
        public const int EntityTypeCode = 4503;

        public customeropportunityrole() : base(EntityLogicalName) { }

        [AttributeLogicalName("customeropportunityroleid")]
        public override System.Guid Id
        {
            get { return base.Id; }
            set { base.Id = value; }
        }

        [AttributeLogicalName("customerid")]
        public EntityReference customerid
        {
            get { return GetAttributeValue<EntityReference>("customerid"); }
            set { SetAttributeValue("customerid", value); }
        }
    }
} 