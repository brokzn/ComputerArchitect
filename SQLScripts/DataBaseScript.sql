use ComputerArchitectDataBase

/*CREATE TABLE CPUS (
	CPUId INT PRIMARY KEY IDENTITY(1,1),
	Product_code VARCHAR(15),
	Cost DECIMAL(10, 2),
	Preview_Photo IMAGE,
	CPU_Count_on_storage INT,
	-- Заводские данные
    Seller_warranty INT,
    Country_of_origin INT,
	-- Общие параметры
    Model VARCHAR(255),
    Socket INT,
    Manufacturer_Id INT,
    Release_year INT,
    Cooling_system_included BIT,
    Thermal_interface VARCHAR(255),
	-- Ядро и архитектура
    Total_cores INT,
    Productive_cores INT,
    Energy_efficient_cores INT,
    Max_threads INT,
    L2_cache_size DECIMAL(3,1),
    L3_cache_size DECIMAL(3,1),
    Technology_process INT,
    Core VARCHAR(255),
	-- Частота и возможность разгона
    Base_processor_speed DECIMAL(3,1),
    Max_turbo_speed DECIMAL(3,1),
    Base_energy_efficient_speed DECIMAL(3,1),
    Turbo_energy_efficient_speed DECIMAL(3,1),
    Unlocked_multiplier BIT,
	-- Параметры оперативной памяти
    Memory_type INT,
    Max_supported_memory INT,
    Number_of_channels INT,
    Memory_speed INT,
    ECC_support BIT,
	-- Тепловые характеристики
    Thermal_design_power INT,
    Base_thermal_design_power INT,
    Max_processor_temperature DECIMAL(4,1),
	-- Графическое ядро
    Integrated_graphics_core BIT,
	-- Шина и контроллеры
    Integrated_PCI_Express_controller INT,
    Number_of_PCI_Express_lines INT,
	-- Дополнительно
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
    MotherboardId INT PRIMARY KEY IDENTITY(1,1), -- Идентификатор
	Cost DECIMAL(10, 2),
	Preview_Photo IMAGE,
	Motherboard_Count_on_storage INT,
    Motherboard_Model VARCHAR(255), -- Модель материнской платы
    Seller_warranty INT, -- Гарантия в месяцах
    Country_of_Origin INT, -- Страна-производитель
    Form_Factor INT, -- Форм-фактор
    Height_Mm INT, -- Высота
    Width_Mm INT, -- Ширина
    Socket INT, -- Тип сокета
    Chipset VARCHAR(255), -- Бренд чипсета
    Compatible_Cpu_Cores VARCHAR(255), -- Совместимые ядра процессоров
    Memory_Type INT, -- Тип памяти
    Memory_Form_Factor VARCHAR(20), -- Форм-фактор памяти
    Memory_Slots INT, -- Количество слотов памяти
    Memory_Channels INT, -- Количество каналов памяти
    Max_Memory_Gb INT, -- Максимальный объем памяти
    Max_Memory_Frequency_Mhz INT, -- Максимальная частота памяти (JEDEC / без разгона)
    Pcie_Version VARCHAR(10), -- Версия PCI Express
    Pcie_X16_Slots INT, -- Количество слотов PCIe x16
    Sli_Crossfire_Support BIT, -- Поддержка SLI / CrossFire
    Pcie_X1_Slots INT, -- Количество слотов PCI-E x1
    Nvme_Support BIT, -- Поддержка NVMe
    M2_Slots INT, -- Количество разъемов M.2
    Sata_Ports INT, -- Количество портов SATA
    Sata_Raid_Support BIT, -- Режим работы SATA RAID
    Ide_Ports INT, -- Количество разъемов IDE
    Rear_Panel_Usb_2_0 INT, -- Количество портов USB 2.0 на задней панели
    Rear_Panel_Usb_Type_C BIT, -- Наличие порта USB Type-C на задней панели
    Rear_Panel_Thunderbolt BIT, -- Наличие порта Thunderbolt на задней панели
    Rear_Panel_Vga BIT, -- Наличие порта VGA на задней панели
    Rear_Panel_Rj45_Ports INT, -- Количество сетевых портов (RJ-45) на задней панели
    Rear_Panel_Analog_Audio_Ports INT, -- Количество аналоговых аудиоразъемов на задней панели
    Rear_Panel_Spdif BIT, -- Наличие цифровых аудиопортов (S/PDIF) на задней панели
    Internal_Usb_2_0 INT, -- Количество внутренних портов USB 2.0
    Internal_Usb_Type_C BIT, -- Наличие внутренних портов USB Type-C
    Cpu_Cooler_Power_Pin INT, -- Разъем питания процессорного охлаждения
    Case_Fan_4_Pin BIT, -- Наличие разъемов для корпусных вентиляторов (4 pin)
    Case_Fan_3_Pin INT, -- Количество разъемов для корпусных вентиляторов (3 pin)
    Argb_Lighting BIT, -- Наличие разъемов 5V-D-G (3 pin) для ARGB подсветки
    Rgb_Lighting BIT, -- Наличие разъемов 12V-G-R-B (4 pin) для RGB подсветки
    M2_Wifi_Module BIT, -- Наличие разъема M.2 (E) для модулей беспроводной связи
    Rs232_Com_Port BIT, -- Наличие разъема RS-232 (COM)
    Lpt_Interface BIT, -- Наличие интерфейса LPT
    Audio_Scheme VARCHAR(50), -- Звуковая схема
    Audio_Chipset VARCHAR(50), -- Чипсет звукового адаптера
    Network_Speed_Gbps DECIMAL(3,1), -- Скорость сетевого адаптера (в Gbps)
    Network_Adapter_Brand VARCHAR(50), -- Бренд сетевого адаптера
    Wifi_Standard VARCHAR(50), -- Стандарт Wi-Fi
    Bluetooth_Version VARCHAR(50), -- Версия Bluetooth
    Main_Power_Connector VARCHAR(50), -- Основной разъем питания
    Cpu_Power_Connector VARCHAR(50), -- Разъем питания процессора
    Power_Phase_Count INT, -- Количество фаз питания
    Passive_Chipset_Cooling BIT, -- Пассивное охлаждение чипсета
    Active_Chipset_Cooling BIT, -- Активное охлаждение чипсета
    Onboard_Buttons BIT, -- Наличие кнопок на плате
    Lighting_Sync_Software BIT, -- Наличие ПО для синхронизации подсветки
    Bundle_Contents VARCHAR(255), -- Комплектация
    Package_Length_Cm DECIMAL(5,2), -- Длина коробки (в см)
    Package_Width_Cm DECIMAL(5,2), -- Ширина коробки (в см)
    Package_Height_Cm DECIMAL(5,2), -- Высота коробки (в см)
    Package_Weight_Kg DECIMAL(5,2) -- Вес с коробкой (в кг)
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