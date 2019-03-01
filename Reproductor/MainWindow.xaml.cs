using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;

using System.Windows.Threading;

namespace Reproductor
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        AudioFileReader reader;
        //Nuestra comunicacion con la tarjeta de sonido
        WaveOutEvent output;

        DispatcherTimer timer;
        VolumeSampleProvider volume;
        FadeInOutSampleProvider fades;

        bool fadingout = false;

        bool dragging = false;

        public MainWindow()
        {
            InitializeComponent();
            LlenarComboSalida();


            //Inicializar timer.
            //Establecer tiempo entre ejecuciones.
            //Establecer lo que se va a ejecutar.
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick; 

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            lbl_tiempoActual.Text = reader.CurrentTime.ToString().Substring(0, 8);
                
        }

        private void LlenarComboSalida()
        {
            cb_salida.Items.Clear();

            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                WaveOutCapabilities capacidades = WaveOut.GetCapabilities(i);
                cb_salida.Items.Add(capacidades.ProductName);
            }
            cb_salida.SelectedIndex = 0;
        }

        private void btn_Archivo(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                txt_archivo.Text = openFileDialog.FileName;
            }
        }

        private void btn_reproducir_Click(Object sender, RoutedEventArgs e)
        {

            if (output != null && output.PlaybackState == PlaybackState.Paused)
            {
                output.Play();
                btn_detener.IsEnabled = true;
                btn_reproducir.IsEnabled = false;
                btn_pausa.IsEnabled = true;


            }
            else
            {
                reader = new AudioFileReader(txt_archivo.Text);

                fades = new FadeInOutSampleProvider(reader, true);

                double milisegundosFadeIn = Double.Parse(txt_DuracionFadeIn.Text) * 1000.0;

                fades.BeginFadeIn(milisegundosFadeIn);

                fadingout = false;

                output = new WaveOutEvent();
        

                output.DeviceNumber = cb_salida.SelectedIndex;
                output.PlaybackStopped += Output_PlaybackStopped;

                
                volume = new VolumeSampleProvider(fades);

                volume.Volume = (float)sld_volumen.Value;

                output.Init(volume);
                output.Play();

                btn_detener.IsEnabled = true;
                btn_reproducir.IsEnabled = true;
                btn_pausa.IsEnabled = true;

                lbl_tiempoTotal.Text = reader.TotalTime.ToString().Substring(0,8);
                lbl_tiempoActual.Text = reader.CurrentTime.ToString().Substring(0, 8);

                timer.Start();
            }

        }

        private void Output_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            reader.Dispose();
            output.Dispose();
            timer.Stop();
        }

        private void btn_pausa_Click(object sender, RoutedEventArgs e)
        {

            if(output != null)
            {
                output.Pause();
                btn_detener.IsEnabled = true;
                btn_reproducir.IsEnabled = true;
                btn_pausa.IsEnabled = true;
            }

        }

        private void btn_detener_Click(object sender, RoutedEventArgs e)
        {
            if( output != null)
            {
                output.Stop();
                btn_detener.IsEnabled = false;
                btn_reproducir.IsEnabled = true;
                btn_pausa.IsEnabled = false;
            }
        }

        private void sld_volumen_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(volume != null && output != null && output.PlaybackState != PlaybackState.Stopped)
            {
                volume.Volume = 
                    (float)sld_volumen.Value;

            }
            if(lbl_porcentajeVolumen != null)
            {
                lbl_porcentajeVolumen.Text = ((int)(sld_volumen.Value * 100)).ToString() + "%";
            }
        }

        private void sld_reproduccion_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dragging = true;

        }

        private void sld_reproduccion_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            dragging = false;

        if (reader != null && output != null && (output.PlaybackState != PlaybackState.Stopped))
            {
                reader.CurrentTime = TimeSpan.FromSeconds(sld_reproduccion.Value);
            }
        }

        private void btn_IniciarFadeOut_Click(object sender, RoutedEventArgs e)
        {
            if (!fadingout && fades != null && output != null && output.PlaybackState == PlaybackState.Playing);
            {
                fadingout = true;
                double milisegundosFadeout = double.Parse(txt_DuracionFadeOut.Text) * 1000.0;
                fades.BeginFadeOut(milisegundosFadeout);
            }
        }

      
    }
}
