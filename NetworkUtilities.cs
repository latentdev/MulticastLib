using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MulticastLib
{
    /// <summary>
    /// Static class with useful network hardware related functions
    /// </summary>
    public static class NetworkUtilities
    {
        /// <summary>
        /// Gets all IPv4 addresses of all adapters
        /// </summary>
        /// <returns>A list of IP addresses</returns>
        public static List<IPAddress> GetLocalIPv4Addresses()
        {
            var localIps = new List<IPAddress>();
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                var ips =
                    nic.GetIPProperties().UnicastAddresses
                        .Select(uni => uni.Address)
                        .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToList();

                localIps.AddRange(ips);
            }

            return localIps;
        }

        /// <summary>
        /// Finds the fastest network interface and returns its IPv4 address
        /// </summary>
        /// <returns>IPv4 address of the fastest interface</returns>
        /// <exception cref="Exception">Throws and exception if there are no active interfaces</exception>
        public static IPAddress GetFastestInterfaceAddress()
        {
            var address = NetworkInterface.GetAllNetworkInterfaces() // get all network interfaces
                .OrderByDescending(x => x.Speed) // sort by speed
                .FirstOrDefault(c => c.NetworkInterfaceType != NetworkInterfaceType.Loopback && c.OperationalStatus == OperationalStatus.Up) // get the first interface that isn't the loopback device and is enabled
                .GetIPProperties()
                .UnicastAddresses
                .Select(uni => uni.Address) // select the IP address
                .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork) // filter for only addressess on the local network
                .FirstOrDefault(); // get the first instance or null if none

            if (address == null) 
                throw new Exception("No active interface found"); // throw an exception if null

            return address; // if the address isn't null return it
        }

        /// <summary>
        /// Gets the IPv4 addresses of all the active(connected) network interfaces on the device except the loopback adapter
        /// </summary>
        /// <returns>An enumerable containing IP addresses of all connected interfaces</returns>
        /// <exception cref="Exception">Throws an exception if there are no active interfaces found</exception>
        public static IEnumerable<IPAddress?> GetConnectedInterfaceAddresses()
        {
            var addresses = NetworkInterface.GetAllNetworkInterfaces()
                .Where(c => c.NetworkInterfaceType != NetworkInterfaceType.Loopback && c.OperationalStatus == OperationalStatus.Up)
                .Select(o =>
                    o.GetIPProperties()
                    .UnicastAddresses
                    .Select(uni => uni.Address)
                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault());

            if (addresses.Count() == 0) throw new Exception("No active interface found");

            return addresses;
        }
    }
}
