using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UnlocksData", menuName = "LowTrace/Unlocks Data")]
public class UnlocksData : ScriptableObject
{
    [System.Serializable]
    public class AutoDesbloqueado
    {
        public string nombre;
        public bool desbloqueado;
        public float tiempoRequerido;
    }

    [System.Serializable]
    public class MapaDesbloqueado
    {
        public string nombre;
        public string escena;
        public bool desbloqueado;
        public float tiempoRequerido;
    }

    public List<AutoDesbloqueado> autos = new List<AutoDesbloqueado>();
    public List<MapaDesbloqueado> mapas = new List<MapaDesbloqueado>();

    public bool EstaAutoDesbloqueado(string nombre)
    {
        foreach (var auto in autos)
        {
            if (auto.nombre == nombre)
                return auto.desbloqueado;
        }
        return false;
    }

    public bool EstaMapaDesbloqueado(string nombre)
    {
        foreach (var mapa in mapas)
        {
            if (mapa.nombre == nombre)
                return mapa.desbloqueado;
        }
        return false;
    }

    public void DesbloquearAuto(string nombre)
    {
        for (int i = 0; i < autos.Count; i++)
        {
            if (autos[i].nombre == nombre)
            {
                var auto = autos[i];
                auto.desbloqueado = true;
                autos[i] = auto;
                return;
            }
        }
    }

    public void DesbloquearMapa(string nombre)
    {
        for (int i = 0; i < mapas.Count; i++)
        {
            if (mapas[i].nombre == nombre)
            {
                var mapa = mapas[i];
                mapa.desbloqueado = true;
                mapas[i] = mapa;
                return;
            }
        }
    }

    public void VerificarDesbloqueos(float mejorTiempo)
    {
        foreach (var auto in autos)
        {
            if (!auto.desbloqueado && auto.tiempoRequerido > 0 && mejorTiempo <= auto.tiempoRequerido)
                DesbloquearAuto(auto.nombre);
        }
        foreach (var mapa in mapas)
        {
            if (!mapa.desbloqueado && mapa.tiempoRequerido > 0 && mejorTiempo <= mapa.tiempoRequerido)
                DesbloquearMapa(mapa.nombre);
        }
    }
}