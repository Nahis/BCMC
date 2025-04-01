using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Client.Services;
using Xrm;
using Microsoft.Xrm.Sdk.Query;
using Lookup = Microsoft.Xrm.Sdk.EntityReference;


using DynamicEntity = Microsoft.Xrm.Sdk.Entity;
using BusinessEntity = Microsoft.Xrm.Sdk.Entity;
using opportunity = Xrm.Opportunity;
using bcmc_hotellead = Xrm.Bcmc_hotellead;
using contact = Xrm.Contact;
using opportunityclose = Xrm.OpportunityClose;
using new_hotel = Xrm.New_hotel;
using systemuser = Xrm.SystemUser;
using new_roompattern = Xrm.New_roompattern;
using bcmc_hotelresponsedetail = Xrm.Bcmc_hotelresponsedetail;
using bcmc_responsehistorylineitem = Xrm.Bcmc_responsehistorylineitem;
using bcmc_responsehistory = Xrm.Bcmc_responsehistory;
using bcmc_portaltemplate = Xrm.Bcmc_portaltemplate;
using msa_eserviceconfiguration = Xrm.MSA_eserviceconfiguration;
using annotation = Xrm.Annotation;
using competitor = Xrm.Competitor;
using bcmc_hotelcontactprofile = Xrm.Bcmc_hotelcontactprofile;
