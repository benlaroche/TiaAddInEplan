using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TiaAddInEplan
{
    public class EplanDevices
    {
        private readonly Project project;

        private readonly string path;

        private readonly FileLogger logger = new FileLogger();

        public readonly List<EplanDevice> eplanDevices = new List<EplanDevice>();

        private readonly string subnetName = "PN/IE_1";

        public IoSystem ioSystem;

        public PlcSoftware plcSoftware;
        public EplanDevices(Project project, string path)
        {
            this.project = project ?? throw new ArgumentNullException("project cannot be null");
            this.path = path ?? throw new ArgumentNullException("path cannot be null");

            SetSubnet();
            CreateEplanDevices();
            SetIoSystem();
            ConnectToIoSystem();
            GetPlcSoftware();
            CreatePlcTags();

        }
        private void SetSubnet()
        {
            if (!project.Subnets.Contains(project.Subnets.Find(subnetName)))
            {
                project.Subnets.Create("System:Subnet.Ethernet", subnetName);
            }


        }
        private void SetIoSystem()
        {
            IoController ioController = (from eplanDevice in eplanDevices
                                         where eplanDevice.NetworkInterface != null
                                         where (eplanDevice.NetworkInterface.InterfaceOperatingMode & InterfaceOperatingModes.IoController) != 0
                                         select eplanDevice.NetworkInterface.IoControllers.First())
            .First();

            if (ioController.IoSystem == null)
                ioSystem = ioController.CreateIoSystem("io system");
            else
                ioSystem = ioController.IoSystem;
        }
        private void CreateEplanDevices()
        {
            var export = from line in File.ReadLines(path).Skip(1)
                         let fields = line.Split(',')
                         select fields;

            foreach (var line in export)
            {
                var device = new EplanDevice(
                    project,
                    line[(int)ExportHeaders.DeviceTagFunctionalAssignment],
                    line[(int)ExportHeaders.DeviceTagHigherLevelFunction],
                    line[(int)ExportHeaders.DeviceTagInstallationSite],
                    line[(int)ExportHeaders.DeviceTagMountingLocation],
                    line[(int)ExportHeaders.DeviceTagDTIdentifier],
                    line[(int)ExportHeaders.DeviceTagDTCounter],
                    line[(int)ExportHeaders.DeviceItemItemSlotOrPort],
                    line[(int)ExportHeaders.FunctionText],
                    line[(int)ExportHeaders.TypeIdentifierType],
                    line[(int)ExportHeaders.Identifier1],
                    line[(int)ExportHeaders.Identifier2],
                    line[(int)ExportHeaders.Identifier3]);


                eplanDevices.Add(device);
            }
        }
        private void ConnectToIoSystem()
        {
            IEnumerable<NetworkInterface> itfs = from eplanDevice in eplanDevices
                                                 where eplanDevice.NetworkInterface != null
                                                 where (eplanDevice.NetworkInterface.InterfaceOperatingMode & InterfaceOperatingModes.IoController) == 0
                                                 where eplanDevice.NetworkInterface.IoConnectors.First().ConnectedToIoSystem == null
                                                 select eplanDevice.NetworkInterface;
            foreach (var itf in itfs)
                itf.IoConnectors.First().ConnectToIoSystem(ioSystem);

        }
        private void GetPlcSoftware()
        {
            plcSoftware = (from eplanDevice in eplanDevices
                             where eplanDevice.NetworkInterface != null
                             where (eplanDevice.NetworkInterface.InterfaceOperatingMode & InterfaceOperatingModes.IoController) != 0
                             select eplanDevice.DeviceItem.GetService<SoftwareContainer>()).First().Software as PlcSoftware;
        }
        private void CreatePlcTags()
        {
            var deviceItemsWithAddresses = new List<DeviceItem>();

            IEnumerable<DeviceItem> deviceItemsWithAddressesToAdd = from eplanDevice in eplanDevices
                                                                    where eplanDevice.DeviceItem.Addresses.Count > 0
                                                                    select eplanDevice.DeviceItem;
            foreach (var item in deviceItemsWithAddressesToAdd)
            {
                deviceItemsWithAddresses.Add(item);
                logger.Log(item.Name);
            }

            IEnumerable<DeviceItem> deviceItemsWithAddressesToAdd2 = from eplanDevice in eplanDevices
                                                                     from eplanDeviceItem in eplanDevice.DeviceItem.DeviceItems
                                                                     where eplanDeviceItem.Addresses.Count > 0
                                                                     select eplanDeviceItem;
            foreach (var item in deviceItemsWithAddressesToAdd2)
            {
                deviceItemsWithAddresses.Add(item);
                logger.Log(item.Name);
            }

            foreach (var deviceItem in deviceItemsWithAddresses)
            {
                foreach (var address in deviceItem.Addresses)
                {
                    if (address.IoType == AddressIoType.Input || address.IoType == AddressIoType.Output)
                    {
                        logger.Log(deviceItem.Name);
                        var startAddress = address.StartAddress;

                        string ioType;
                        if (address.IoType == AddressIoType.Input)
                            ioType = "I";
                        else
                            ioType = "Q";

                        var length = address.Length;

                        if (length <= 8)
                        {
                            for (int i = 0; i < length; i++)
                            {
                                var name = deviceItem.Name + "X" + i.ToString();
                                var logicalAddress = "%" + ioType + startAddress.ToString() + "." + i.ToString();
                                CreatePlcTag(name, "Bool", logicalAddress);
                            }
                        }

                        if (length > 8)
                        {
                            for (int i = 0; i < length/8; i+=2)
                            {
                                var name = deviceItem.Name + ioType + "W" + i.ToString();
                                var logicalAddress = "%" + ioType + "W" + (startAddress + i).ToString();
                                CreatePlcTag(name, "Word", logicalAddress);
                            }
                        }
                    }
                }
            }
        }
        private void CreatePlcTag(string name, string dataTypeName, string logicalAddress)
        {
            
            if (plcSoftware.TagTableGroup.TagTables[0].Tags.Contains(plcSoftware.TagTableGroup.TagTables[0].Tags.Find(name)))
                return;
            logger.Log("create plc tag");
            try
            {
                plcSoftware.TagTableGroup.TagTables[0].Tags.Create(name, dataTypeName, logicalAddress);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
        }
    }
}