using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Threading;

namespace SetIP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string EthDns ;
        string EthIP;
        string EthName;
               
        string WifiIP;
        string WifiDns;
        string WifiName;
               
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                this.Loaded += new RoutedEventHandler(Window_Loaded);   //To show window in lower right corner


                InitializeButtons();

                DispatcherTimer timer = new DispatcherTimer();   // Timer to update button colors every 10 sec
                timer.Interval = TimeSpan.FromSeconds(10);
                timer.Tick += timer_Tick;
                timer.Start();


               

            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }

        }


        void timer_Tick(object sender, EventArgs e)
        {
            InitializeButtons();
        }

        void InitializeButtons()
        {
            EthernetInf(out EthIP, out EthDns, out EthName);
            WifiInf(out WifiIP, out WifiDns, out WifiName);



            EthStatic.ClearValue(Button.BackgroundProperty);
            EthDHCP.ClearValue(Button.BackgroundProperty);

            WIFIStatic.ClearValue(Button.BackgroundProperty);
            WIFIDhcp.ClearValue(Button.BackgroundProperty);


            if (EthIP == "" && EthDns == "")
            {
                return;

            }

            else if (EthIP == Properties.Settings.Default.EthIPac)
            {
                EthStatic.Background = Brushes.Cyan;

            }


            else
            {
                EthDHCP.Background = Brushes.Cyan;

            }
            //................................

            if (WifiIP == "" && WifiDns == "")
            {
                return;

            }

            else if (WifiIP == Properties.Settings.Default.WifiIPac)
            {
                WIFIStatic.Background = Brushes.Cyan;

            }
            else
            {
                WIFIDhcp.Background = Brushes.Cyan;

            }
        }




        private void Window_Loaded(object sender, RoutedEventArgs e) //To show window in lower right corner
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }




        private void SetIP(Button sender, string arg)  //To set IP with elevated cmd prompt
        {
            try
            {
                if (sender.Background == Brushes.Cyan )
                { 
                    MessageBox.Show("Already Selected...");
                    return;
                }
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe");
                psi.UseShellExecute = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.Verb = "runas";
                psi.Arguments = arg;
                Process.Start(psi);
                if (sender == EthStatic ||  sender == EthDHCP )
                {
                    EthStatic.ClearValue(Button.BackgroundProperty);
                    EthDHCP.ClearValue(Button.BackgroundProperty);

                    sender.Background = Brushes.Cyan;
                 }

                if (sender == WIFIStatic || sender == WIFIDhcp)
                {
                    WIFIStatic.ClearValue(Button.BackgroundProperty);
                    WIFIDhcp.ClearValue(Button.BackgroundProperty);

                    sender.Background = Brushes.Cyan;
                }

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                
            }


        }

      





        private static void EthernetInf(out string ip, out string dns, out string nic)  // To get current ethernet config
        {
            ip =  "" ;
            dns = "";
            nic = "";
            string[] NwDesc = { "TAP", "VMware", "Windows", "Virtual" };  // Adapter types (Description) to be ommited
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet && !NwDesc.Any(ni.Description.Contains))  // check for adapter type and its description
                {

                    foreach (IPAddress dnsAdress in ni.GetIPProperties().DnsAddresses)
                    {
                        if (dnsAdress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            dns = dnsAdress.ToString();
                        }
                    }



                    foreach (UnicastIPAddressInformation ips in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ips.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !ips.Address.ToString().StartsWith("169")) //to exclude automatic ips
                        {
                            ip = ips.Address.ToString();
                            nic = ni.Name;
                        }
                    }
                }
            }
            
        }   

        



        private static void WifiInf(out string ip, out string dns, out string nic)  // To get current wifi config
        {
            ip = "";
            dns = "";
            nic = "";
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {

                    foreach (IPAddress dnsAdress in ni.GetIPProperties().DnsAddresses)
                    {
                        if (dnsAdress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            dns = dnsAdress.ToString();
                        }
                    }


                    foreach (UnicastIPAddressInformation ips in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ips.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !ips.Address.ToString().StartsWith("169")) //to exclude automatic ips
                        {
                            ip = ips.Address.ToString();
                            nic = ni.Name;
                        }
                    }
                }
            }
            
        }




        private void EthStatic_Click(object sender, RoutedEventArgs e)
        {
            
            SetIP(EthStatic, "/c netsh interface ip set address \"" + EthName + "\" static " + Properties.Settings.Default.EthIPac + " " + Properties.Settings.Default.Subnet + " " + Properties.Settings.Default.EthDnsac + " & netsh interface ip set dns \"" + EthName + "\" static " + Properties.Settings.Default.EthDnsac);
            

        }


        private void EthDHCP_Click(object sender, RoutedEventArgs e)
        {

            SetIP( EthDHCP, "/c netsh interface ip set address \"" + EthName + "\" dhcp & netsh interface ip set dns \"" + EthName + "\" dhcp");
            

        }

      

        private void WIFIStatic_Click(object sender, RoutedEventArgs e)
        {
            
            SetIP(WIFIStatic, "/c netsh interface ip set address \"" + WifiName + "\" static " + Properties.Settings.Default.WifiIPac + " " + Properties.Settings.Default.Subnet + " " + Properties.Settings.Default.WifiDnsac + " & netsh interface ip set dns \"" + WifiName + "\" static " + Properties.Settings.Default.WifiDnsac);
            
        }

        private void WIFIDhcp_Click(object sender, RoutedEventArgs e)
        {
            SetIP(WIFIDhcp, "/c netsh interface ip set address \"" + WifiName + "\" dhcp & netsh interface ip set dns \"" + WifiName + "\" dhcp");

        }
    }
}