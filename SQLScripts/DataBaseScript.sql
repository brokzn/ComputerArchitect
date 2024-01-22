use ComputerArchitectDataBase

/*CREATE TABLE CPUS (
	CPUId INT PRIMARY KEY IDENTITY(1,1),
	Product_code VARCHAR(15),
	Cost DECIMAL(10, 2),
	Preview_Photo IMAGE,
	CPU_Count_on_storage INT,
	-- ��������� ������
    Seller_warranty INT,
    Country_of_origin INT,
	-- ����� ���������
    Model VARCHAR(255),
    Socket INT,
    Manufacturer_Id INT,
    Release_year INT,
    Cooling_system_included BIT,
    Thermal_interface VARCHAR(255),
	-- ���� � �����������
    Total_cores INT,
    Productive_cores INT,
    Energy_efficient_cores INT,
    Max_threads INT,
    L2_cache_size DECIMAL(3,1),
    L3_cache_size DECIMAL(3,1),
    Technology_process INT,
    Core VARCHAR(255),
	-- ������� � ����������� �������
    Base_processor_speed DECIMAL(3,1),
    Max_turbo_speed DECIMAL(3,1),
    Base_energy_efficient_speed DECIMAL(3,1),
    Turbo_energy_efficient_speed DECIMAL(3,1),
    Unlocked_multiplier BIT,
	-- ��������� ����������� ������
    Memory_type INT,
    Max_supported_memory INT,
    Number_of_channels INT,
    Memory_speed INT,
    ECC_support BIT,
	-- �������� ��������������
    Thermal_design_power INT,
    Base_thermal_design_power INT,
    Max_processor_temperature DECIMAL(4,1),
	-- ����������� ����
    Integrated_graphics_core BIT,
	-- ���� � �����������
    Integrated_PCI_Express_controller INT,
    Number_of_PCI_Express_lines INT,
	-- �������������
    Virtualization_technology BIT,
    Additional_info VARCHAR(255)
);

CREATE TABLE Sockets (
	SocketId INT PRIMARY KEY IDENTITY(1,1),
	SocketName VARCHAR(20)
);

CREATE TABLE Memory_types (
	Memory_typeId INT PRIMARY KEY IDENTITY(1,1),
	Memory_typeName VARCHAR(10)
);

CREATE TABLE Manufacturers (
	ManufacturersId INT PRIMARY KEY IDENTITY(1,1),
	ManufacturersName VARCHAR(100),
	ManufacturersLogo IMAGE
);

CREATE TABLE Motherboards (
    MotherboardId INT PRIMARY KEY IDENTITY(1,1), -- �������������
	Cost DECIMAL(10, 2),
	Preview_Photo IMAGE,
	Motherboard_Count_on_storage INT,
    Motherboard_Model VARCHAR(255), -- ������ ����������� �����
    Seller_warranty INT, -- �������� � �������
    Country_of_Origin INT, -- ������-�������������
    Form_Factor INT, -- ����-������
    Height_Mm INT, -- ������
    Width_Mm INT, -- ������
    Socket INT, -- ��� ������
    Chipset VARCHAR(255), -- ����� �������
    Compatible_Cpu_Cores VARCHAR(255), -- ����������� ���� �����������
    Memory_Type INT, -- ��� ������
    Memory_Form_Factor VARCHAR(20), -- ����-������ ������
    Memory_Slots INT, -- ���������� ������ ������
    Memory_Channels INT, -- ���������� ������� ������
    Max_Memory_Gb INT, -- ������������ ����� ������
    Max_Memory_Frequency_Mhz INT, -- ������������ ������� ������ (JEDEC / ��� �������)
    Pcie_Version VARCHAR(10), -- ������ PCI Express
    Pcie_X16_Slots INT, -- ���������� ������ PCIe x16
    Sli_Crossfire_Support BIT, -- ��������� SLI / CrossFire
    Pcie_X1_Slots INT, -- ���������� ������ PCI-E x1
    Nvme_Support BIT, -- ��������� NVMe
    M2_Slots INT, -- ���������� �������� M.2
    Sata_Ports INT, -- ���������� ������ SATA
    Sata_Raid_Support BIT, -- ����� ������ SATA RAID
    Ide_Ports INT, -- ���������� �������� IDE
    Rear_Panel_Usb_2_0 INT, -- ���������� ������ USB 2.0 �� ������ ������
    Rear_Panel_Usb_Type_C BIT, -- ������� ����� USB Type-C �� ������ ������
    Rear_Panel_Thunderbolt BIT, -- ������� ����� Thunderbolt �� ������ ������
    Rear_Panel_Vga BIT, -- ������� ����� VGA �� ������ ������
    Rear_Panel_Rj45_Ports INT, -- ���������� ������� ������ (RJ-45) �� ������ ������
    Rear_Panel_Analog_Audio_Ports INT, -- ���������� ���������� ������������� �� ������ ������
    Rear_Panel_Spdif BIT, -- ������� �������� ����������� (S/PDIF) �� ������ ������
    Internal_Usb_2_0 INT, -- ���������� ���������� ������ USB 2.0
    Internal_Usb_Type_C BIT, -- ������� ���������� ������ USB Type-C
    Cpu_Cooler_Power_Pin INT, -- ������ ������� ������������� ����������
    Case_Fan_4_Pin BIT, -- ������� �������� ��� ��������� ������������ (4 pin)
    Case_Fan_3_Pin INT, -- ���������� �������� ��� ��������� ������������ (3 pin)
    Argb_Lighting BIT, -- ������� �������� 5V-D-G (3 pin) ��� ARGB ���������
    Rgb_Lighting BIT, -- ������� �������� 12V-G-R-B (4 pin) ��� RGB ���������
    M2_Wifi_Module BIT, -- ������� ������� M.2 (E) ��� ������� ������������ �����
    Rs232_Com_Port BIT, -- ������� ������� RS-232 (COM)
    Lpt_Interface BIT, -- ������� ���������� LPT
    Audio_Scheme VARCHAR(50), -- �������� �����
    Audio_Chipset VARCHAR(50), -- ������ ��������� ��������
    Network_Speed_Gbps DECIMAL(3,1), -- �������� �������� �������� (� Gbps)
    Network_Adapter_Brand VARCHAR(50), -- ����� �������� ��������
    Wifi_Standard VARCHAR(50), -- �������� Wi-Fi
    Bluetooth_Version VARCHAR(50), -- ������ Bluetooth
    Main_Power_Connector VARCHAR(50), -- �������� ������ �������
    Cpu_Power_Connector VARCHAR(50), -- ������ ������� ����������
    Power_Phase_Count INT, -- ���������� ��� �������
    Passive_Chipset_Cooling BIT, -- ��������� ���������� �������
    Active_Chipset_Cooling BIT, -- �������� ���������� �������
    Onboard_Buttons BIT, -- ������� ������ �� �����
    Lighting_Sync_Software BIT, -- ������� �� ��� ������������� ���������
    Bundle_Contents VARCHAR(255), -- ������������
    Package_Length_Cm DECIMAL(5,2), -- ����� ������� (� ��)
    Package_Width_Cm DECIMAL(5,2), -- ������ ������� (� ��)
    Package_Height_Cm DECIMAL(5,2), -- ������ ������� (� ��)
    Package_Weight_Kg DECIMAL(5,2) -- ��� � �������� (� ��)
);*/
CREATE TABLE GPUS (
    GPUId INT PRIMARY KEY IDENTITY(1,1),
    Cost DECIMAL(10, 2),
    Preview_Photo IMAGE,
    GPU_Count_on_storage INT,
    GPU_Model VARCHAR(255),
    Seller_Warranty INT,
    Country_of_Origin INT,
    Manufacturer_Id INT,
    Color VARCHAR(50),
    Mining_Purpose BIT,
    LHR BIT,
    GPU_Processor VARCHAR(150),
    Microarchitecture VARCHAR(150),
    Technology_Process_nm INT,
    GPU_Base_Frequency_MHz INT,
    ALU_Count INT,
    Texture_Block_Count INT,
    Rasterization_Block_Count INT,
    Ray_Tracing_Support BIT,
    RT_Cores INT,
    Tensor_Cores BIT,
    Video_Memory_Size_GB INT,
    Video_Memory_Type VARCHAR(20),
    Memory_Bus_Width_Bits INT,
    Memory_Bandwidth_GBps DECIMAL(5,2),
    Memory_Frequency_MHz INT,
    Video_Output_Type_and_Count VARCHAR(255),
    HDMI_Version VARCHAR(10),
    Max_Monitors_Supported INT,
    Max_Resolution VARCHAR(20),
    Connection_Interface VARCHAR(20),
    Connector_Form_Factor VARCHAR(20),
    PCI_Express_Lanes INT,
    Additional_Power_Connectors BIT,
    Recommended_Power_Supply_Watt INT,
    Cooling_Type VARCHAR(50),
    Fan_Type_and_Count VARCHAR(255),
    Liquid_Cooling_Radiator BIT,
    RGB_Lighting BIT,
    RGB_Sync BIT,
    LCD_Display BIT,
    BIOS_Switch BIT,
    Low_Profile BIT,
    Occupied_Expansion_Slots INT,
    Length_mm INT,
    Width_mm INT,
    Thickness_mm INT,
    Weight_g INT
);