$path = 'C:\DotNetProjects\FlightJobsDesktop\FlightJobs.Presentation\Views\Home\ConnectorView.xaml.cs'
$lines = [System.IO.File]::ReadAllLines($path)
$output = New-Object System.Collections.Generic.List[string]

for ($i = 0; $i -lt $lines.Length; $i++) {
    $line = $lines[$i]
    
    # 1. Adicionar evento SimConnected no construtor
    if ($line.Contains('_airportDataProvider = MainWindow.AirportDataProviderFactory.Create();')) {
        $output.Add($line)
        $output.Add('            FlightJobsConnectSim.SimConnected += OnSimConnected;')
        continue
    }

    # 2. Corrigir LoadUserJobData: não tentar ler Laty/Lonx se for null
    if ($line.Contains('var arrivalEntity = await _airportDataProvider.GetAirportAsync(_currentJob.ArrivalICAO);')) {
        $output.Add('                    // Tenta obter coordenadas do cache local')
        $output.Add('                    bool hasNavdata = UpdateJobCoordinatesFromCache();')
        $output.Add('                    _currentJob.IsNavdataLoaded = hasNavdata;')
        $output.Add('')
        $output.Add('                    // Se o simulador já estiver conectado e não tivermos navdata carregado, pergunta via modal')
        $output.Add('                    if (FlightJobsConnectSim.CommonSimData.IsConnected && !hasNavdata && !_isJobStarted)')
        $output.Add('                    {')
        $output.Add('                        Dispatcher.BeginInvoke((Action)(async () => await ShowLoadNavdataDialog()));')
        $output.Add('                    }')
        
        # Pular as linhas antigas que causavam NullReferenceException (linhas 763 a 774)
        while ($i -lt $lines.Length -and !$lines[$i].Contains('if (_siderJobWindow != null) _siderJobWindow.Close();')) {
            $i++
        }
        $output.Add('                    if (_siderJobWindow != null) _siderJobWindow.Close();')
        continue
    }

    # 3. Corrigir StartIsEnable: se não tiver navdata carregado, deve iniciar bloqueado (false)
    if ($line.Contains('_currentJob.StartIsEnable = !_isJobStarted;')) {
        $output.Add('                    _currentJob.StartIsEnable = !_isJobStarted && _currentJob.IsNavdataLoaded;')
        continue
    }

    # 4. Adicionar handler do botão manual btnLoadNavdata_Click antes do fim da classe
    if ($line.Trim() -eq '}' -and $i -eq ($lines.Length - 1)) {
        $output.Add('        private async void btnLoadNavdata_Click(object sender, RoutedEventArgs e)')
        $output.Add('        {')
        $output.Add('            await ShowLoadNavdataDialog();')
        $output.Add('        }')
        $output.Add($line)
        continue
    }

    $output.Add($line)
}

[System.IO.File]::WriteAllLines($path, $output)
Write-Output "ConnectorView.xaml.cs updated successfully"

