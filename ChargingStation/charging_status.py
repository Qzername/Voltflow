
import smbus

# Define I2C address for the INA3221 (0x40 by default)
INA3221_ADDRESS = 0x40

# Define registers
INA3221_REG_CONFIG = 0x00
INA3221_REG_SHUNT_VOLTAGE_1 = 0x01
INA3221_REG_BUS_VOLTAGE_1 = 0x02
INA3221_REG_SHUNT_VOLTAGE_2 = 0x03
INA3221_REG_BUS_VOLTAGE_2 = 0x04
INA3221_REG_SHUNT_VOLTAGE_3 = 0x05
INA3221_REG_BUS_VOLTAGE_3 = 0x06

# Initialize I2C (I2C bus 1 for Raspberry Pi)
bus = smbus.SMBus(1)

def read_bus_voltage(channel):
    """Read the bus voltage for a given channel"""
    if channel == 1:
        voltage_reg = INA3221_REG_BUS_VOLTAGE_1
    elif channel == 2:
        voltage_reg = INA3221_REG_BUS_VOLTAGE_2
    elif channel == 3:
        voltage_reg = INA3221_REG_BUS_VOLTAGE_3
    else:
        raise ValueError("Invalid channel")

    # Read 2 bytes of data (16-bit)
    raw_data = bus.read_word_data(INA3221_ADDRESS, voltage_reg)
    # Swap the bytes (because of endianness)
    raw_data = ((raw_data << 8) & 0xFF00) + (raw_data >> 8)
    # Convert the data to voltage (in mV)
    voltage = raw_data * 0.001
    return voltage

def read_shunt_voltage(channel):
    """Read the shunt voltage for a given channel"""
    if channel == 1:
        shunt_reg = INA3221_REG_SHUNT_VOLTAGE_1
    elif channel == 2:
        shunt_reg = INA3221_REG_SHUNT_VOLTAGE_2
    elif channel == 3:
        shunt_reg = INA3221_REG_SHUNT_VOLTAGE_3
    else:
        raise ValueError("Invalid channel")

    # Read 2 bytes of data (16-bit)
    raw_data = bus.read_word_data(INA3221_ADDRESS, shunt_reg)
    # Swap the bytes (because of endianness)
    raw_data = ((raw_data << 8) & 0xFF00) + (raw_data >> 8)
    # Convert the data to voltage (in mV)
    shunt_voltage = raw_data * 0.0025  # 2.5uV per bit
    return shunt_voltage

def read_current(channel):
    """Calculate the current from the shunt voltage"""
    shunt_voltage = read_shunt_voltage(channel)
    # Current = Shunt Voltage / Shunt Resistance
    # The INA3221 uses a 0.1 ohm resistor for the shunt, so we divide by 0.1 to get current in Amps.
    current = shunt_voltage / 0.1  # In Amps
    return current

def get_info():
    result = []

    for channel in range(1,3):
        #voltage = read_bus_voltage(channel)
        current = read_current(channel)

        power = 5 * current / 1000
        result.append(power)

    return result