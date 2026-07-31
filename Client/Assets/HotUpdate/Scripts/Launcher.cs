using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using UnityEngine;

public partial class Launcher : MonoBehaviour
{
    private IServiceProvider serviceProvider;
    private void Awake()
    {
        serviceProvider = new Configure().Build();
        Ioc.Default.ConfigureServices(serviceProvider);
    }
}
