using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml.Serialization;
using Crestron.SimplSharp.CrestronIO;

namespace SEFrameworkcfg
{

    public class XMLEntry
     {
        
         //Event Handlers
         public event EventHandler<UshrtChangeEventArgs> UOutputChange;
         public event EventHandler<StringChangeEventArgs> StringChange;
        
         //Lists
         private List<Entry>ReadEntries;

         //Constants
         public const string analogType = "analog";
         public const string serialType = "serial";

         // Used locally once modified by S+
         public string _filePath = "\\NVRAM\\Test\\Config.xml";             // temporary static path for testing
         public ushort _debug = 0;
        
         //Values S+ modifies
         public string filePath
         {
             get { return _filePath; }
             set { _filePath = value; }
         }                                               // XML file path set from S+
         public ushort debug                                                     // debug variable set from S+
         {
             get { return _debug; }
             set { _debug = value; }
         }

         public void GetEntry()                                                    // Reads file and passes data to S+
         {            
             try
             {
                 if (File.Exists(filePath))                                      // Check for existing file
                 {
                     if(_debug == 1) CrestronConsole.PrintLine("File Found: " + filePath + "\n");
                     ReadEntries = CrestronXMLSerialization.DeSerializeObject<List<Entry>>(filePath);     // Populate the list with deserialized data
                     if (_debug == 1) CrestronConsole.PrintLine("{0} Read and Deserialized!\n", filePath);

                     int i = 0;
                    
                     foreach (Entry item in ReadEntries)
                     {
                        
                         if (_debug == 1) CrestronConsole.PrintLine(String.Format("Entry " + (i + 1) + ": Name: " + ReadEntries[i].Name +" Type: " + ReadEntries[i].Type + " Value: " + ReadEntries[i].Value + "\n"));
                         i++;
                     }
                     SendValues();
                 }
                                                                                 // If no file is found, create one at the specified path with sample data
                 else
                 {
                     if (_debug == 1) CrestronConsole.PrintLine("File Not Found. Creating: " + filePath + "\n");

                     List<Entry> SampleValues = new List<Entry>();                      
            
                     for (int i = 1; i <= 5; i++)                                // Populate the SampleValues list with values to serve as an example structure to modify manually
                     {
                         Entry tempEntry = new Entry();
                  
                         SampleValues.Add(new Entry() {Name = String.Format( "AnalogName {0}", i), Type = analogType, Value = string.Format( "{0}", i)});
                     }

                     for (int i = 1; i <= 5; i++)                                // Populate the SampleValues list with values to serve as an example structure to modify manually
                     {
                         Entry tempEntry = new Entry();

                         SampleValues.Add(new Entry() { Name = String.Format("SerialName {0}", i), Type = serialType, Value = string.Format("{0}", i) });
                     }
                     CrestronXMLSerialization.SerializeObject(_filePath, SampleValues);
                     if (_debug == 1) CrestronConsole.PrintLine(filePath + " Successfully Created.\n");
                     GetEntry();
                   }
             }
             catch(Exception e)
             {
                 if (_debug == 1) CrestronConsole.PrintLine("Error Loading or Creating XML File." + e);
             }
            
         }

       
         public void SendValues()                                                // Sends values from the ReadEntries List object to S+
         {
             ushort a = 1;                       // Counters for each type of entry
             ushort s = 1;
             int i = 0;
            
             foreach (Entry item in ReadEntries)
             {
                 if (ReadEntries[i].Type == analogType)
                 {
                    
                     OnIndexedUshortChange(Convert.ToUInt16(ReadEntries[i].Value), a, ReadEntries[i].Type, ReadEntries[i].Name);
                     a++;
                 }
                 else if (ReadEntries[i].Type == serialType)
                 {
                    
                     OnIndexedStringChange(ReadEntries[i].Value, s, ReadEntries[i].Type, ReadEntries[i].Name);
                     s++;
                 }
                 i++;
             }
         }

         public class Entry      // Base class for XML Node
         {
             public string Name {get; set;}
             public string Type {get; set;}
             public string Value {get; set;}
         }

         // Event creation helpers
         void OnIndexedUshortChange(ushort value, ushort index, string type, string name)
         {
             if (UOutputChange != null)
                 UOutputChange(this, new UshrtChangeEventArgs { IntValue = value, Index = index, sType = type, EntryName = name});
         }

         void OnIndexedStringChange(string value, ushort index, string type, string name)
         {
             if (StringChange != null)
                 StringChange(this, new StringChangeEventArgs { StringValue = value, Index = index, sType = type, EntryName = name});
         }
     }

     public class UshrtChangeEventArgs : EventArgs
     {
         public ushort IntValue { get; set; }
         public ushort Index { get; set; }
         public string sType { get; set; }
         public string EntryName { get; set; }
     }

     public class StringChangeEventArgs : EventArgs
     {
         public string StringValue { get; set; }
         public ushort Index { get; set; }
         public string sType { get; set; }
         public string EntryName { get; set; }
     }

    public class MeetingPresets
    {
        //Event Handlers
       
        public event EventHandler<SourceChangeEventArgs> SourceChange;
        public event EventHandler<DestinationChangeEventArgs> DestinationChange;
        public event EventHandler<ShareChangeEventArgs> ShareChange;
        public event EventHandler<ConfigurationChangeEventArgs> ConfigurationChange;

        
        //Lists
        private List<Preset> ReadPresets;

       public ushort PresetCount = 0;
       public const ushort maxPresetCount = 9;                                     // Base 0 means 10 presets max
        
        // Used locally once modified by S+
        public string filePath                                                      // XML file path set from S+
        {
            get { return _filePath; }
            set { _filePath = value; }
        }                                               
        public ushort debug                                                         // debug variable set from S+
        {
            get { return _debug; }
            set { _debug = value; }
        }

        public ushort SelectPreset;
        public string _filePath = "\\NVRAM\\Test\\Presets.xml";             // temporary static path for testing
        public ushort _debug = 0;
        public ushort _PresetID = 1;                                        // Default Preset Value at Startup


        public ushort PresetID                                                      // PresetID variable set from S+
        {
            get { return _PresetID; }
            set { _PresetID = value; }
        }

       
        public void GetPresets()                                                    // Reads presets from file if present or creates file with sample preset values if not
        {
            try
            {
                if (File.Exists(filePath))                                      // Check for existing file
                {
                    if (_debug == 1) CrestronConsole.PrintLine("File Found: " + filePath + "\n");
                    ReadPresets = CrestronXMLSerialization.DeSerializeObject<List<Preset>>(filePath);     // Populate the list with deserialized data
                    if (_debug == 1) CrestronConsole.PrintLine("{0} Read and Deserialized!\n", filePath);

                    int i = 0;


                    foreach (Preset item1 in ReadPresets)
                    {

                        if (_debug == 1) CrestronConsole.PrintLine(String.Format("ID: " + ReadPresets[i].ID + " Name: " + ReadPresets[i].Name + "\n"));
                        
                        int j = 0;
                        int k = 0;
                        int l = 0;

                        foreach (Destination item2 in ReadPresets[i].Destination)
                        {
                            CrestronConsole.PrintLine(String.Format(" Name: " + ReadPresets[i].Destination[j].Name +
                                                                    " Source: " + ReadPresets[i].Destination[j].Value + "\n"));
                            j++;
                        }

                        foreach (Source item3 in ReadPresets[i].Source)
                        {
                            CrestronConsole.PrintLine(String.Format("Name: " + ReadPresets[i].Source[k].Name +
                                                                    " Video: " + ReadPresets[i].Source[k].Video +
                                                                    " Audio: " + ReadPresets[i].Source[k].Audio +
                                                                    " Icon: " + ReadPresets[i].Source[k].Icon +
                                                                    " Control: " + ReadPresets[i].Source[k].Control +
                                                                    " Share: " + ReadPresets[i].Source[k].Share + "\n"));
                            k++;
                        }
                        foreach (Share item4 in ReadPresets[i].Share)
                        {
                            CrestronConsole.PrintLine(String.Format("Name: " + ReadPresets[i].Share[l].Name +
                                                                    "Video: " + ReadPresets[i].Share[l].Video +
                                                                    "Audio: " + ReadPresets[i].Share[l].Audio +
                                                                    "Icon: " + ReadPresets[i].Share[l].Icon +
                                                                    "Control: " + ReadPresets[i].Share[l].Control + "\n"));
                            l++;
                        }
                        i++;
                    }
                    RecallPreset(PresetID, 0);
                }
                else                // If no file is found, create one at the specified path with sample data
                {
                    if (_debug == 1) CrestronConsole.PrintLine("File Not Found. Creating: " + filePath + "\n");

                    List<Preset> SamplePreset = new List<Preset>();

                    List<Destination> SampleDestinations = new List<Destination>();

                    List<Source> SampleSources = new List<Source>();

                    List<Share> SampleShares = new List<Share>();

                    for (int i = 1; i <= 4; i++)        // Populate the SampleValues
                    {
                        SampleSources.Add(new Source() { Name = "Source" + (ushort)i, 
                                                        Video = (ushort)i, 
                                                        Audio = (ushort)i, 
                                                        Icon = Convert.ToUInt16("166"), 
                                                        Control = Convert.ToUInt16("0"), 
                                                        Share = Convert.ToUInt16("1") });
                    }

                    for (int i = 1; i <= 4; i++)        // Populate the SampleValues
                    {
                        SampleShares.Add(new Share()
                        {
                            Name = "Shared Source" + (ushort)i,
                            Video = (ushort)i,
                            Audio = (ushort)i,
                            Icon = Convert.ToUInt16("166"),
                            Control = Convert.ToUInt16("0")});
                    }

                    for (int i = 1; i <= 4; i++)        // Populate the SampleValues
                    {
                        SampleDestinations.Add(new Destination() { Name = "Destination" + (ushort)i, 
                                                           Value=(ushort)i});
                    }


                    for (int i = 0; i <= 5; i++)        // Populate the SampleValues list with values to serve as an example structure to modify manually
                    {
                        SamplePreset.Add(new Preset() { ID = (ushort)i, 
                                                        Name = "Example Preset"+(ushort)i,
                                                        RoomName = "RoomName",
                                                        ACNumber = "(xxx) yyy-zzzz",
                                                        ACMode = 0,
                                                        VCNumber = "My VC Number",
                                                        VCMode = 0,
                                                        PRESMode = 1,
                                                        CalendarMode=0,
                                                        LIGHTMode = 1,
                                                        OCCUPANCYVisible = 0,
                                                        HALLWAYVisible = 0,
                                                        SKYFOLDVisible = 0,
                                                        SourceMax = 4,
                                                        DestinationMax = 4,
                                                        SharingMax = 4,
                                                        SharingInputPos = 5,
                                                        LightZoneMax = 1,
                                                        ShadeZoneMax = 3,
                                                        ShadeCfg = 1,
                                                        ConferenceMicMax = 16,
                                                        OtherMicMax = 2,
                                                        CombineMax = 0,
                                                        ScreenMax = 0,
                                                        LiftMax = 0,
                                                        VCLocalCamera = 1,
                                                        VCLocalCameraEnable = 1,
                                                        VCSpeakerTrack = 0,
                                                        VCPreset = 3,
                                                        AutoSwitching = 0,
                                                        AudioFollow = 0,
                                                        VolumeModeBasic = 0,
                                                        AdvanceDisable = 1,
                                                        LanguageDisable = 0,
                                                        LanguageDefault = 0,
                                                        Destination = SampleDestinations, 
                                                        Source = SampleSources,
                                                        Share = SampleShares});
                    }

                    SerializeList(SamplePreset);
                    GetPresets();       // Recall the File to propagate the Sample Value to system.
                    
                }
            }
            catch (Exception e)
            {
                if (_debug == 1) CrestronConsole.PrintLine("Error Loading or Creating XML File." + e);
            }

        }
        
        public void SerializeList(List<Preset> tempList)
        {
            CrestronXMLSerialization.SerializeObject(_filePath, tempList);
            CrestronConsole.PrintLine(filePath + " Successfully Written to File.\n");
        }                         // Writes the List to an .xml file



        public void SetPresetSelect(ushort index)
        {
            SelectPreset = index;                                                   // Assign parameter and account for base 1 to base 0 conversion of index

            if (SelectPreset <= (ushort)ReadPresets.Count)                      // If passed index matches a preset in the list, read out the values
            {
                RecallPreset(SelectPreset, 0);                                      // convert index param back to base 1
            }
        }
                                     
        public void RecallPreset(ushort index, ushort execute)                                                  // Sends values of selected preset to S+
        {
            index--;                                                                                            // account for base 1 to base 0 conversion of index   

            SelectPreset = index;
            
            if (_debug == 1) CrestronConsole.PrintLine("Recalling Preset values at index {0}.  Execute: {1}\n", index, execute);


            OnIndexedConfigurationChange(ReadPresets[index].ID,
                                         ReadPresets[index].Name,
                                         ReadPresets[index].RoomName,
                                         ReadPresets[index].ACNumber,
                                         ReadPresets[index].ACMode,
                                         ReadPresets[index].VCNumber,
                                         ReadPresets[index].VCMode,
                                         ReadPresets[index].PRESMode,
                                         ReadPresets[index].CalendarMode,
                                         ReadPresets[index].LIGHTMode,
                                         ReadPresets[index].OCCUPANCYVisible,
                                         ReadPresets[index].HALLWAYVisible,
                                         ReadPresets[index].SKYFOLDVisible,
                                         ReadPresets[index].SourceMax,
                                         ReadPresets[index].DestinationMax,
                                         ReadPresets[index].SharingMax,
                                         ReadPresets[index].SharingInputPos,
                                         ReadPresets[index].LightZoneMax,
                                         ReadPresets[index].ShadeZoneMax,
                                         ReadPresets[index].ShadeCfg,
                                         ReadPresets[index].ConferenceMicMax,
                                         ReadPresets[index].OtherMicMax,
                                         ReadPresets[index].CombineMax,
                                         ReadPresets[index].ScreenMax,
                                         ReadPresets[index].LiftMax,
                                         ReadPresets[index].VCLocalCamera,
                                         ReadPresets[index].VCLocalCameraEnable,
                                         ReadPresets[index].VCSpeakerTrack,
                                         ReadPresets[index].VCPreset,
                                         ReadPresets[index].AutoSwitching,
                                         ReadPresets[index].AudioFollow,
                                         ReadPresets[index].VolumeModeBasic,
                                         ReadPresets[index].AdvanceDisable,
                                         ReadPresets[index].LanguageDisable,
                                         ReadPresets[index].LanguageDefault);

            ushort i = 0;

            foreach (Source item in ReadPresets[index].Source)
            {
                OnIndexedSourceChange(i,
                                      ReadPresets[index].Source[i].Name,
                                      ReadPresets[index].Source[i].Video,
                                      ReadPresets[index].Source[i].Audio,
                                      ReadPresets[index].Source[i].Icon,
                                      ReadPresets[index].Source[i].Control,
                                      ReadPresets[index].Source[i].Share);
                i++;
            }

            i = 0;

            foreach (Share item in ReadPresets[index].Share)
            {
                OnIndexedShareChange(i,
                                      ReadPresets[index].Share[i].Name,
                                      ReadPresets[index].Share[i].Video,
                                      ReadPresets[index].Share[i].Audio,
                                      ReadPresets[index].Share[i].Icon,
                                      ReadPresets[index].Share[i].Control);
                i++;
            }

            i = 0;

            foreach (Destination item in ReadPresets[index].Destination)
            {
                OnIndexedDestinationChange(i, 
                                       ReadPresets[index].Destination[i].Name,
                                       ReadPresets[index].Destination[i].Value,
                                       ReadPresets[index].Destination[i].NoSourceTimer);
                i++;
            }
            
        }



        public class Destination    // XML Display Node
        {
            //public ushort ID { get; set; }
            public string Name { get; set; }
            public ushort Value { get; set; }
            public ushort NoSourceTimer { get; set; }
        }

        public class Source       // XML Source Node
        {
            //public ushort ID { get; set; }
            public string Name { get; set; }
            public ushort Video { get; set; }
            public ushort Audio { get; set; }
            public ushort Icon { get; set; }
            public ushort Control { get; set; }
            public ushort Share { get; set; }
        }

        public class Share       // XML Share Node
        {
            //public ushort ID { get; set; }
            public string Name { get; set; }
            public ushort Video { get; set; }
            public ushort Audio { get; set; }
            public ushort Icon { get; set; }
            public ushort Control { get; set; }
        }

        public class Preset     // Base class for XML Preset Node
        {
            public ushort ID { get; set; }
            public string Name { get; set; }
            public string RoomName { get; set; }
            public string ACNumber { get; set; }
            public ushort ACMode { get; set; }
            public string VCNumber { get; set; }
            public ushort VCMode { get; set; }
            public ushort PRESMode { get; set; }
            public ushort CalendarMode { get; set; }
            public ushort LIGHTMode { get; set; }
            public ushort OCCUPANCYVisible { get; set; }
            public ushort HALLWAYVisible { get; set; }
            public ushort SKYFOLDVisible { get; set; }
            public ushort SourceMax { get; set; }
            public ushort DestinationMax { get; set; }
            public ushort SharingMax { get; set; }
            public ushort SharingInputPos { get; set; }
            public ushort LightZoneMax { get; set; }
            public ushort ShadeZoneMax { get; set; }
            public ushort ShadeCfg { get; set; }
            public ushort ConferenceMicMax { get; set; }
            public ushort OtherMicMax { get; set; }
            public ushort CombineMax { get; set; }
            public ushort ScreenMax { get; set; }
            public ushort LiftMax { get; set; }
            public ushort VCLocalCamera { get; set; }
            public ushort VCLocalCameraEnable { get; set; }
            public ushort VCSpeakerTrack { get; set; }
            public ushort VCPreset { get; set; }
            public ushort AutoSwitching { get; set; }
            public ushort AudioFollow { get; set; }
            public ushort VolumeModeBasic { get; set; }
            public ushort AdvanceDisable { get; set; }
            public ushort LanguageDisable { get; set; }
            public ushort LanguageDefault { get; set; }
            public List<Destination> Destination;
            public List<Source> Source;
            public List<Share> Share;
        }

        // Event creation helpers

        void OnIndexedSourceChange( ushort index,
                                    string name,
                                    ushort video,
                                    ushort audio,
                                    ushort icon,
                                    ushort control,
                                    ushort share)
        {
            if (SourceChange != null)
                SourceChange(this, new SourceChangeEventArgs { Index = index,
                                                               Name = name,
                                                               Video = video,
                                                               Audio = audio,
                                                               Icon = icon,
                                                               Control = control,
                                                               Share = share });
        }

        void OnIndexedShareChange(  ushort index,
                                    string name,
                                    ushort video,
                                    ushort audio,
                                    ushort icon,
                                    ushort control)
        {
            if (ShareChange != null)
                ShareChange(this, new ShareChangeEventArgs
                {
                    Index = index,
                    Name = name,
                    Video = video,
                    Audio = audio,
                    Icon = icon,
                    Control = control,
                });
        }


        void OnIndexedDestinationChange(ushort index,
                                        string name,
                                        ushort value,
                                        ushort nosourcetimer)
        {
            if (DestinationChange != null)
                DestinationChange(this, new DestinationChangeEventArgs { Index = index,
                                                                         Name = name,
                                                                         Value = value,
                                                                         NoSourceTimer = nosourcetimer});
        }

        void OnIndexedConfigurationChange(  ushort index,
                                            string name,
                                            string roomname,
                                            string acnumber,
                                            ushort acmode,
                                            string vcnumber,
                                            ushort vcmode,
                                            ushort presmode,
                                            ushort calendarmode,
                                            ushort lightmode,
                                            ushort occupancyvisible,
                                            ushort hallwayvisible,
                                            ushort skyfoldvisible,
                                            ushort sourcemax,
                                            ushort destinationmax,
                                            ushort sharingmax,
                                            ushort sharinginputpos,
                                            ushort lightzonemax,
                                            ushort shadezonemax,
                                            ushort shadecfg,
                                            ushort conferencemicmax,
                                            ushort othermicmax,
                                            ushort combinemax,
                                            ushort screenmax,
                                            ushort liftmax,
                                            ushort vclocalcamera,
                                            ushort vclocalcameraenable,
                                            ushort vcspeakertrack,
                                            ushort vcpreset,
                                            ushort autoswitching,
                                            ushort audiofollow,
                                            ushort volumemodebasic,
                                            ushort advancedisable,
                                            ushort languagedisable,
                                            ushort languagedefault)
        {
            if (ConfigurationChange != null)
                ConfigurationChange(this, new ConfigurationChangeEventArgs { Index = index,
                                                                             Name = name,
                                                                             RoomName = roomname,
                                                                             ACNumber = acnumber,
                                                                             ACMode = acmode,
                                                                             VCNumber = vcnumber,
                                                                             VCMode = vcmode,
                                                                             PRESMode = presmode,
                                                                             CalendarMode = calendarmode,
                                                                             LIGHTMode = lightmode,
                                                                             OCCUPANCYVisible = occupancyvisible,
                                                                             HALLWAYVisible = hallwayvisible,
                                                                             SKYFOLDVisible = skyfoldvisible,
                                                                             SourceMax = sourcemax,
                                                                             DestinationMax = destinationmax,
                                                                             SharingMax = sharingmax,
                                                                             SharingInputPos = sharinginputpos,
                                                                             LightZoneMax = lightzonemax,
                                                                             ShadeZoneMax = shadezonemax,
                                                                             ShadeCfg = shadecfg,
                                                                             ConferenceMicMax = conferencemicmax,
                                                                             OtherMicMax = othermicmax,
                                                                             CombineMax = combinemax,
                                                                             ScreenMax = screenmax,
                                                                             LiftMax = liftmax,
                                                                             VCLocalCamera = vclocalcamera,
                                                                             VCLocalCameraEnable = vclocalcameraenable,
                                                                             VCSpeakerTrack = vcspeakertrack,
                                                                             VCPreset = vcpreset,
                                                                             AutoSwitching = autoswitching,
                                                                             AudioFollow = audiofollow,
                                                                             VolumeModeBasic = volumemodebasic,
                                                                             AdvanceDisable = advancedisable,
                                                                             LanguageDisable = languagedisable,
                                                                             LanguageDefault = languagedefault});
        }

    }

    public class SourceChangeEventArgs :  EventArgs
    {
        public ushort Index { get; set; }
        public string Name  { get; set; }
        public ushort Video { get; set; }
        public ushort Audio { get; set; }
        public ushort Icon { get; set; }
        public ushort Control { get; set; }
        public ushort Share { get; set; }


    }

    public class ShareChangeEventArgs : EventArgs
    {
        public ushort Index { get; set; }
        public string Name { get; set; }
        public ushort Video { get; set; }
        public ushort Audio { get; set; }
        public ushort Icon { get; set; }
        public ushort Control { get; set; }
    }

    public class DestinationChangeEventArgs : EventArgs
    {
        public ushort Index { get; set; }
        public string Name { get; set; }
        public ushort Value { get; set; }
        public ushort NoSourceTimer { get; set; }
    }

    public class ConfigurationChangeEventArgs : EventArgs
    {
        public ushort Index { get; set; }
        public string Name { get; set; }
        public string RoomName { get; set; }
        public string ACNumber { get; set; }
        public ushort ACMode { get; set; }
        public string VCNumber { get; set; }
        public ushort VCMode { get; set; }
        public ushort PRESMode { get; set; }
        public ushort CalendarMode { get; set; }
        public ushort LIGHTMode { get; set; }
        public ushort OCCUPANCYVisible { get; set; }
        public ushort SKYFOLDVisible { get; set; }
        public ushort HALLWAYVisible { get; set; }
        public ushort SourceMax { get; set; }
        public ushort DestinationMax { get; set; }
        public ushort SharingMax { get; set; }
        public ushort SharingInputPos { get; set; }
        public ushort LightZoneMax { get; set; }
        public ushort ShadeZoneMax { get; set; }
        public ushort ShadeCfg{get;set;}
        public ushort ConferenceMicMax { get; set; }
        public ushort OtherMicMax { get; set; }
        public ushort CombineMax { get; set; }
        public ushort ScreenMax { get; set; }
        public ushort LiftMax { get; set; }
        public ushort VCLocalCamera { get; set; }
        public ushort VCLocalCameraEnable { get; set; }
        public ushort VCSpeakerTrack { get; set; }
        public ushort VCPreset { get; set; }
        public ushort AutoSwitching { get; set; }
        public ushort AudioFollow { get; set; }
        public ushort VolumeModeBasic { get; set; }
        public ushort AdvanceDisable { get; set; }
        public ushort LanguageDisable { get; set; }
        public ushort LanguageDefault { get; set; }
    }

 
}