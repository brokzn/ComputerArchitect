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
);
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

CREATE TABLE RAMS (
    RAMId INT PRIMARY KEY IDENTITY(1,1),
    Cost DECIMAL(10, 2),
    Preview_Photo IMAGE,
    RAM_Count_on_storage INT,
    RAM_Model VARCHAR(255),
    Seller_Warranty INT,
    Country_of_Origin INT,
	Manufacturer_Id INT,
	Memory_Type INT,
    RAM_Form_Factor VARCHAR(20),
    Capacity_GB INT,
    Registered_Memory BIT,
    ECC_Memory BIT,
    RAM_Speed_MHz INT,
    AMD_EXPO_Profile BIT,
    Intel_XMP_Profile BIT,
    CAS_Latency INT,
    RAS_to_CAS_Delay INT,
    Row_Precharge_Delay INT,
    Activate_to_Precharge_Delay INT,
    RAM_Heatsink BIT,
    RGB_Lighting BIT,
    RAM_Height_mm INT,
    Low_Profile BIT,
    RAM_Voltage DECIMAL(3, 1)
);

CREATE TABLE PowerSupplies (
    PowerSupplyId INT PRIMARY KEY IDENTITY(1,1),
    Cost DECIMAL(10, 2),
	Preview_Photo IMAGE,
	PS__Count_on_storage INT,
    Seller_Warranty INT,
    Country_of_Origin INT,
    Model VARCHAR(255),
    Manufacturer_Id INT,
    Power_Watt INT,
    Form_Factor VARCHAR(50),
    Color VARCHAR(50),
    Detachable_Cables BIT,
    Cable_Sleeving BIT,
    Cable_Colors VARCHAR(50),
    LED_Type VARCHAR(50),
    LED_Color VARCHAR(50),
    Main_Power_Connector VARCHAR(50),
    CPU_Power_Connector VARCHAR(50),
    GPU_Power_Connector VARCHAR(50),
    SATA_Connector_Count INT,
    Molex_Connector_Count INT,
    Floppy_Connector BIT,
    Power_12V_Watt INT,
    Power_12V1_A INT,
    Power_12V2_A INT,
    Power_3_3V_A INT,
    Power_5V_A INT,
    Power_5V_Standby_A INT,
    Power_minus12V_A INT,
    Input_Voltage_Range VARCHAR(50),
    Cooling_System VARCHAR(50),
    Fan_Size VARCHAR(50),
    Max_Noise_Level DECIMAL(5, 2),
    Certification_80PLUS BIT,
    Power_Factor_Correction BIT,
    Standards_Compliance VARCHAR(50),
    Protection_Technologies VARCHAR(50),
    Length_mm INT,
    Width_mm INT,
    Height_mm INT,
    Weight_kg DECIMAL(5, 2)
);

CREATE TABLE Cases (
    CaseId INT PRIMARY KEY IDENTITY(1,1),
    Cost DECIMAL(10, 2),
    Preview_Photo IMAGE,
	Cases_Count_on_storage INT,
    Seller_Warranty INT,
    Country_of_Origin INT,
    Model VARCHAR(255),
    Manufacturer_Id INT,
    Case_Size INT,
    Motherboard_Orientation VARCHAR(20),
    Length_mm INT,
    Width_mm INT,
    Height_mm INT,
    Weight_kg DECIMAL(5, 2),
    Main_Color VARCHAR(50),
    Case_Material VARCHAR(100),
    Side_Panel_Window BIT,
    Window_Material VARCHAR(50),
    Front_Panel_Material VARCHAR(50),
    RGB_Lighting_Type VARCHAR(50),
    RGB_Lighting_Color VARCHAR(50),
    RGB_Connection_Type VARCHAR(50),
    RGB_Control_Method VARCHAR(100),
    Compatible_Motherboard_Form_Factors VARCHAR(100),
    Compatible_PSU_Form_Factors VARCHAR(8),
    PSU_Placement VARCHAR(20),
    Max_PSU_Length_mm INT,
    Horizontal_Expansion_Slots INT,
    Vertical_Expansion_Slots INT,
    Max_GPU_Length_mm INT,
    Max_CPU_Cooler_Height_mm INT,
    Internal_2_5_Drive_Bays INT,
    Internal_3_5_Drive_Bays INT,
    External_3_5_Drive_Bays INT,
    External_5_25_Drive_Bays INT,
    Cooling_Fans_Included INT,
    Front_Fan_Support BIT,
    Rear_Fan_Support BIT,
    Top_Fan_Support BIT,
    Bottom_Fan_Support BIT,
    Side_Fan_Support BIT,
    Liquid_Cooling_Support BIT,
    Top_Radiator_Size_mm INT,
    Rear_Radiator_Size_mm INT,
    Bottom_Radiator_Size_mm INT,
    Front_Panel_IO_Location VARCHAR(20),
    Front_Panel_IO_Ports VARCHAR(255),
    Built_in_PSU BIT,
    Low_Noise_Anti_Vibration_Cases BIT,
    Case_Fixing_Method VARCHAR(50),
    Cable_Management_Holes BIT,
    Cable_Cover_Plate BIT,
    Dust_Filter BIT,
    Additional_Features VARCHAR(255),
    Included_Accessories VARCHAR(255)
);

CREATE TABLE Coolers (
    CoolerId INT PRIMARY KEY IDENTITY(1,1),
    Cost DECIMAL(10, 2),
	Preview_Photo IMAGE,
	Cooler_Count_on_storage INT,
    Seller_Warranty INT,
    Country_of_Origin INT,
    Cooler_Type VARCHAR(50),
    Model VARCHAR(255),
    Manufacturer_Id INT,
    Socket_Type INT,
    TDP INT,
    Construction_Type VARCHAR(20),
    Base_Material VARCHAR(50),
    Radiator_Material VARCHAR(50),
    Heat_Pipes_Count INT,
    Nickel_Plating BIT,
    Radiator_Color VARCHAR(50),
    Fan_Count INT,
    Max_Fan_Count INT,
    Fan_Size VARCHAR(20),
    Fan_Color VARCHAR(50),
    Fan_Connector_Type VARCHAR(10),
    Max_Fan_Speed_RPM INT,
    Min_Fan_Speed_RPM INT,
    Fan_Speed_Control BIT,
    Max_Airflow_CFM DECIMAL(8, 2),
    Max_Noise_Level_dB DECIMAL(8, 2),
    Nominal_Current_Amps DECIMAL(5, 2),
    Nominal_Voltage INT,
    Bearing_Type VARCHAR(50),
    Thermal_Paste_Included BIT,
    Lighting_Type VARCHAR(50),
    Accessories VARCHAR(255),
    Height_mm DECIMAL(8, 2),
    Width_mm DECIMAL(8, 2),
    Length_mm DECIMAL(8, 2),
    Weight_g DECIMAL(8, 2)
);

CREATE TABLE SSDs (
    SSDId INT PRIMARY KEY IDENTITY(1,1),
    Cost DECIMAL(10, 2),
    Preview_Photo IMAGE,
    SSD_Count_on_storage INT,
    Seller_Warranty INT,
    Country_of_Origin INT,
    SSD_Type INT,
    Model VARCHAR(255),
    Manufacturer_Id INT,
    Volume_GB INT,
    NVMe BIT,
    Connection_Type VARCHAR(10),
    Bits_per_Cell INT,
    Memory_Structure VARCHAR(20),
    DRAM_Buffer BIT,
    Max_Sequential_Read_Speed_MBs INT,
    Max_Sequential_Write_Speed_MBs INT,
    Total_Bytes_Written_TB DECIMAL(8, 2),
    Drive_Writes_Per_Day DECIMAL(5, 2),
    Hardware_Encryption BIT,
    Width_mm DECIMAL(8, 2),
    Length_mm DECIMAL(8, 2),
    Thickness_mm DECIMAL(8, 2),
    Weight_g DECIMAL(8, 2)
);*/

CREATE TABLE HDDs (
    HDDId INT PRIMARY KEY IDENTITY(1,1),
    Cost DECIMAL(10, 2),
    Preview_Photo IMAGE,
    HDD_Count_on_storage INT,
    Seller_Warranty INT,
    Country_of_Origin INT,
    HDD_Type VARCHAR(50),
    Model VARCHAR(255),
    Manufacturer_Id INT,
    Capacity_TB INT,
    Cache_MB INT,
    Spindle_Speed_RPM INT,
    Max_Data_Transfer_Speed_MBs INT,
    Average_Latency_ms DECIMAL(5, 2),
    Interface VARCHAR(20),
    Interface_Bandwidth_GBps DECIMAL(4, 2),
    RAID_Optimization BIT,
    Recording_Technology VARCHAR(50),
    Operational_Shock_Grating_G INT,
    Operating_Noise_Level_dBA INT,
    Idle_Noise_Level_dBA INT,
    Helium_Filling BIT,
    Positioning_Parking_Cycles INT,
    Max_Power_Consumption_Watts DECIMAL(5, 2),
    Idle_Power_Consumption_Watts DECIMAL(5, 2),
    Max_Operating_Temperature_Celsius INT,
    Width_mm DECIMAL(8, 2),
    Length_mm DECIMAL(8, 2),
    Thickness_mm DECIMAL(8, 2),
    Weight_g DECIMAL(8, 2)
);
