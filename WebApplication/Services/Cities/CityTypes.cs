using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace OpenRiaServices.Client.Benchmarks.Server.Cities
{


    /// <summary>
    /// These types are simple data types that can be used to build
    /// mocks and simple data stores.
    /// </summary>
    /// <remarks>
    /// These types have no static dependencies on Silverlight, but they
    /// are declared as 'partial' types so they may be extended after inclusion
    /// into other compilation units.
    /// </remarks>
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Cities")]
    public partial class State
    {
        private readonly List<County> _counties = new List<County>();

        [DataMember] public string Name {get; set; }
        [DataMember] public string FullName {get; set; }
        [DataMember] public TimeZone TimeZone {get; set; }
        [DataMember] public ShippingZone ShippingZone {get; set; }
        [DataMember] public List<County> Counties { get { return this._counties; } }
    }

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Cities")]
    public partial class County
    {
        public County()
        {
            Cities = new List<City>();
        }

        [DataMember] public string Name {get; set; }
        [DataMember] public string StateName {get; set; }
        [DataMember] public State State {get; set; }

        [DataMember] public List<City> Cities { get; set; }
    }

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Cities")]
    [KnownType(typeof(CityWithEditHistory))]
    [KnownType(typeof(CityWithInfo))]
    [JsonDerivedType(typeof(CityWithEditHistory))]

    public partial class City
    {
        public City()
        {
            ZipCodes = new List<Zip>();
        }

        [JsonInclude] [DataMember] public string Name {get; set; }
        [JsonInclude] [DataMember] public string CountyName {get; set; }
        [JsonInclude] [DataMember] public string StateName {get; set; }
        [JsonInclude] [DataMember] public County County {get; set; }
        [JsonInclude] [DataMember] public string ZoneName {get; set; }
        [JsonInclude] [DataMember] public string CalculatedCounty {get { return this.CountyName; } set { } }
        [JsonInclude] [DataMember] public int ZoneID {get; set; }


        [JsonInclude] [DataMember] public List<Zip> ZipCodes { get; set; }

        public override string ToString()
        {
            return this.GetType().Name + " Name=" + this.Name + ", State=" + this.StateName + ", County=" + this.CountyName;
        }

        public int this[int index]
        {
            get
            {
                return index;
            }
            set
            {
            }
        }
    }

    // This class introduces an abstract derived class in the
    // City hierarchy that allows CUD and Custom methods to
    // record when they executed.  We do not update the history
    // with normal property sets, only with explicit domain
    // operations.
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Cities")]
    [JsonDerivedType(typeof(CityWithInfo))]
    public /*abstract*/ partial class CityWithEditHistory : City
    {
        private string _editHistory;

        public CityWithEditHistory()
        {
            this.EditHistory = "new";
        }

        [JsonInclude]
        [DataMember]
        // Edit history always appends, never overwrites
        public string EditHistory
        {
            get
            {
                return this._editHistory;
            }
            set
            {
                this._editHistory = this._editHistory == null ? value : (this._editHistory + "," + value);
                this.LastUpdated = DateTime.Now;
            }
        }

        [JsonInclude]
        [DataMember]
         public DateTime LastUpdated
        {
            get;
            set;
        }

        public override string ToString()
        {
            return base.ToString() + ", History=" + this.EditHistory + ", Updated=" + this.LastUpdated;
        }

    }

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Cities")]
    public partial class CityWithInfo : CityWithEditHistory
    {
        public CityWithInfo()
        {
            ZipCodesWithInfo = new List<ZipWithInfo>();
        }

        [JsonInclude]
        [DataMember]
        public string Info
        {
            get;
            set;
        }

        [JsonInclude]
        [DataMember]
        public List<ZipWithInfo> ZipCodesWithInfo { get; set; }

        public override string ToString()
        {
            return base.ToString() + ", Info=" + this.Info;
        }

    }

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Cities")]
    [KnownType(typeof(ZipWithInfo))]
    [JsonDerivedType(typeof(ZipWithInfo))]
    public partial class Zip
    {
        [JsonInclude] [DataMember] public int Code {get; set; }
        [JsonInclude][DataMember] public int FourDigit {get; set; }
        [JsonInclude][DataMember] public string CityName {get; set; }
        [JsonInclude][DataMember] public string CountyName {get; set; }
        [JsonInclude][DataMember] public string StateName {get; set; }

        [JsonInclude][DataMember] public City City {get; set; }
    }

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Cities")]
    public partial class ZipWithInfo : Zip
    {
        [JsonInclude][DataMember]
        public string Info
        {
            get;
            set;
        }
    }
}
