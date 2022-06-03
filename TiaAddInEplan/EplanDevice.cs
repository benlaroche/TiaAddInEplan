
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.HW.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TiaAddInEplan
{


    public class EplanDevice
    {
        public string DeviceTagFunctionalAssignment { get; set; }

        public string DeviceTagHigherLevelFunction { get; set; }

        public string DeviceTagInstallationSite { get; set; }

        public string DeviceTagMountingLocation { get; set; }

        public string DeviceTagDTIdentifier { get; set; }

        public string DeviceTagDTCounter { get; set; }

        public string DeviceItemItemSlotOrPort { get; set; }

        public string FunctionText { get; set; }

        public string TypeIdentifierType { get; set; }

        public string Identifier1 { get; set; }

        public string Identifier2 { get; set; }

        public string Identifier3 { get; set; }
        //---------------------------------------------------------------------------------------------
        public string TypeIdentifier { get; set; }

        public enum TypeIdentifierTypes
        {
            OrderNumber,
            GSD,
            System
        }
        public struct OrderNumberTypeIdentifier
        {
            string orderNumber;
            string firmewareVersion;
            string additionalTypeIdentification;
        }

        public struct GsdTypeIdentifier
        {
            string gsdName;
            string gsdType;
            string gsdID;
        }

        public enum GsdType
        {
            D, //device
            R,//rack
            DAP,//headmodule
            M,//module
            SM//submodule
        }

        public struct SystemTypeIdentifier
        {
            string systemTypeIdentifier;
            string additionalTypeIdentifier;
        }

        public string DeviceName { get; set; }
        public string DeviceItemName { get; set; }
        public Device Device { get; set; }

        public GsdDevice GsdDevice { get; set; }
        public DeviceItem DeviceItem { get; set; }
        public DeviceUserGroup DeviceUserGroup { get; private set; }

        private readonly Project project;

        private readonly FileLogger logger = new FileLogger();

        public Subnet Subnet { get; private set; }

        public NetworkInterface NetworkInterface { get; private set; }

        public EplanDevice(
                Project project,
                string DeviceTagFunctionalAssignment,
                string DeviceTagHigherLevelFunction,
                string DeviceTagInstallationSite,
                string DeviceTagMountingLocation,
                string DeviceTagDTIdentifier,
                string DeviceTagDTCounter,
                string DeviceItemItemSlotOrPort,
                string FunctionText,
                string TypeIdentifierType,
                string Identifier1,
                string Identifier2,
                string Identifier3
                )
        {
            this.project = project ?? throw new ArgumentNullException("project cannot be null");
            this.DeviceTagFunctionalAssignment = DeviceTagFunctionalAssignment;
            this.DeviceTagHigherLevelFunction = DeviceTagHigherLevelFunction;
            this.DeviceTagInstallationSite = DeviceTagInstallationSite;
            this.DeviceTagMountingLocation = DeviceTagMountingLocation;
            this.DeviceTagDTIdentifier = DeviceTagDTIdentifier;
            this.DeviceTagDTCounter = DeviceTagDTCounter;
            this.DeviceItemItemSlotOrPort = DeviceItemItemSlotOrPort;
            this.FunctionText = FunctionText;
            this.TypeIdentifierType = TypeIdentifierType;
            this.Identifier1 = Identifier1;
            this.Identifier2 = Identifier2;
            this.Identifier3 = Identifier3;

            SetDeviceUserGroup();
            SetTypeIdentifier();
            SetDeviceItemName();
            SetDeviceName();
            SetDevice();
            SetGsdDevice();
            SetDeviceItem();
            ConnectToSubnet();
        }
        private void ConnectToSubnet()
        {
            if (Convert.ToInt32(DeviceItemItemSlotOrPort) == 0)
            {
                this.Subnet = project.Subnets.First();
                foreach (var deviceItem in DeviceItem.DeviceItems)
                {
                    NetworkInterface itf = ((IEngineeringServiceProvider)deviceItem).GetService<NetworkInterface>();
                    if (itf != null && itf.InterfaceType == NetType.Ethernet)
                    {
                        NetworkInterface = itf;
                        if (itf.Nodes.First().ConnectedSubnet == null)
                            NetworkInterface.Nodes.First().ConnectToSubnet(Subnet);
                        else if (itf.Nodes.First().ConnectedSubnet.Name != Subnet.Name)
                            NetworkInterface.Nodes.First().ConnectToSubnet(Subnet);
                        break;
                    }
                }

            }

        }
        private void SetGsdDevice()
        {

            if (TypeIdentifierType == TypeIdentifierTypes.GSD.ToString())
            {
                this.GsdDevice = Device.GetService<GsdDevice>();
            }


        }
        private void SetDevice()
        {
            this.Device = DeviceUserGroup.Devices.Find(DeviceName);
            if (DeviceUserGroup.Devices.Contains(Device))
                return;
            CreateDeviceWithItem();
        }
        private void SetDeviceItem()
        {
            foreach (var deviceItem in Device.DeviceItems)
            {
                if (deviceItem.Name == DeviceItemName)
                {
                    this.DeviceItem = deviceItem;
                    return;
                }
            }
            CreateDeviceItem();
        }
        private void CreateDeviceItem()
        {
            if (Identifier2 == GsdType.SM.ToString())
            {
                var deviceItem = FindFirstGsdM(FindFirstGsdDAP().DeviceItems);
                if (deviceItem.CanPlugNew(TypeIdentifier, DeviceItemName, Convert.ToInt32(DeviceItemItemSlotOrPort)))
                {
                    this.DeviceItem = deviceItem.PlugNew(TypeIdentifier, DeviceItemName, Convert.ToInt32(DeviceItemItemSlotOrPort));
                    return;
                }
            }
            this.DeviceItem = Device.Items[0].PlugNew(TypeIdentifier, DeviceItemName, Convert.ToInt32(DeviceItemItemSlotOrPort));
        }
        private DeviceItem FindFirstGsdDAP()
        {
            foreach (var deviceItem in Device.DeviceItems)
            {
                if (Convert.ToString(deviceItem.GetAttribute("GsdType")) == GsdType.DAP.ToString())
                    return deviceItem;
            }
            return null;
        }
        private DeviceItem FindFirstGsdM(DeviceItemComposition deviceItemComposition)
        {
            foreach (var deviceItem in deviceItemComposition)
            {
                if (Convert.ToString(deviceItem.GetAttribute("GsdType")) == GsdType.M.ToString())
                    return deviceItem;
            }
            return null;
        }
        private void CreateDeviceWithItem()
        {
            try
            {
                this.Device = DeviceUserGroup.Devices.CreateWithItem(TypeIdentifier, DeviceItemName, DeviceName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void SetDeviceItemName()
        {
            DeviceItemName = String.Join("-", new string[]{
                DeviceTagFunctionalAssignment,
                DeviceTagHigherLevelFunction,
                DeviceTagInstallationSite,
                DeviceTagMountingLocation,
                DeviceTagDTIdentifier,
                DeviceTagDTCounter,
                DeviceItemItemSlotOrPort});
        }
        private void SetDeviceName()
        {
            DeviceName = String.Join("-", new string[]{
                DeviceTagFunctionalAssignment,
                DeviceTagHigherLevelFunction,
                DeviceTagInstallationSite,
                DeviceTagMountingLocation,
                DeviceTagDTIdentifier,
                DeviceTagDTCounter});
        }
        private void SetTypeIdentifier()
        {

            if (TypeIdentifierType == TypeIdentifierTypes.OrderNumber.ToString())
            {
                if (Identifier2 == "" && Identifier3 == "")
                {

                    HardwareUtilityComposition extensions = project.HwUtilities;
                    var result = extensions.Find("ModuleInformationProvider") as ModuleInformationProvider;

                    try
                    {
                        string partialTypeIdentifier = TypeIdentifierType + ":" + Identifier1;
                        string[] moduleTypes = result.FindModuleTypes(partialTypeIdentifier);
                        TypeIdentifier = moduleTypes[moduleTypes.Length - 1];
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else
                    TypeIdentifier = TypeIdentifierType + ":" + Identifier1 + "/" + Identifier2 + "/" + Identifier3;
            }
            else
                TypeIdentifier = TypeIdentifierType + ":" + Identifier1.ToUpper() + "/" + Identifier2 + "/" + Identifier3;
        }
        private void SetDeviceUserGroup()
        {
            var names = new string[]{DeviceTagFunctionalAssignment,
                                    DeviceTagHigherLevelFunction,
                                    DeviceTagInstallationSite,
                                    DeviceTagMountingLocation,
                                    DeviceTagDTIdentifier};
            DeviceUserGroupComposition deviceUserGroupComposition = project.DeviceGroups;
            DeviceUserGroup deviceUserGroup = null;
            foreach (var name in names)
            {
                deviceUserGroup = CreateDeviceUserGroup(deviceUserGroupComposition, name);
                deviceUserGroupComposition = deviceUserGroup.Groups;
            }

            DeviceUserGroup = deviceUserGroup;
        }
        private DeviceUserGroup CreateDeviceUserGroup(DeviceUserGroupComposition deviceUserGroupComposition, string name)
        {
            var deviceUserGroup = FindDeviceUserGroup(deviceUserGroupComposition, name);
            if (deviceUserGroup == null)
            {
                try
                {
                    return deviceUserGroupComposition.Create(name);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return deviceUserGroup;
        }
        private DeviceUserGroup FindDeviceUserGroup(DeviceUserGroupComposition deviceUserGroupComposition, string name)
        {
            var deviceUserGroup = deviceUserGroupComposition.Find(name);
            if (deviceUserGroupComposition.Contains(deviceUserGroup))
                return deviceUserGroup;
            return null;
        }

    }

}