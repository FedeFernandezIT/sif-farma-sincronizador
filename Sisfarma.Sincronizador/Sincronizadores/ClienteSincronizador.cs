﻿using Sisfarma.Sincronizador.Extensions;
using Sisfarma.Sincronizador.Farmatic;
using Sisfarma.Sincronizador.Fisiotes;
using Sisfarma.Sincronizador.Fisiotes.Models;
using Sisfarma.Sincronizador.Helpers;
using Sisfarma.Sincronizador.Sincronizadores.SuperTypes;
using System;

namespace Sisfarma.Sincronizador.Sincronizadores
{
    public class ClienteSincronizador : TaskSincronizador
    {
        private readonly bool _hasSexo;

        private string _puntosDeSisfarma;
        private bool _perteneceFarmazul;

        public ClienteSincronizador(FarmaticService farmatic, FisiotesService fisiotes)
            : base(farmatic, fisiotes)
        {
            _hasSexo = farmatic.Clientes.HasSexoField();
        }

        public override void Process() => ProcessClientes();

        public override void LoadConfiguration()
        {
            _puntosDeSisfarma = ConfiguracionPredeinida[Configuracion.FIELD_PUNTOS_SISFARMA];
            _perteneceFarmazul = _fisiotes.Configuraciones.PerteneceFarmazul();
        }

        public override void PreSincronizacion()
            => _fisiotes.Clientes.ResetDniTracking();

        public void ProcessClientes()
        {
            var localClientes = _farmatic.Clientes.GetAll();

            var contadorHuecos = -1;
            foreach (var cliente in localClientes)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                if (contadorHuecos == -1)
                    contadorHuecos = Convert.ToInt32(cliente.IDCLIENTE);

                InsertOrUpdateCliente(cliente);

                var intIdCliente = Convert.ToInt32(cliente.IDCLIENTE);
                if (intIdCliente != contadorHuecos)
                {
                    for (int i = contadorHuecos; i < intIdCliente; i++)
                    {
                        if (!_fisiotes.Huecos.Any(i))
                            _fisiotes.Huecos.Insert(i.ToString());
                    }
                    contadorHuecos = intIdCliente;
                }
                contadorHuecos++;
            }
        }

        private void InsertOrUpdateCliente(Farmatic.Models.Cliente cliente)
        {
            var clienteDTO = Generator.GenerarCliente(_farmatic, cliente, _hasSexo);

            var debeCargarPuntos = _puntosDeSisfarma.ToLower().Equals("no") || string.IsNullOrWhiteSpace(_puntosDeSisfarma);

            var dniCliente = cliente.PER_NIF.Strip();

            if (_perteneceFarmazul)
            {
                var beBlue = _farmatic.Clientes.EsBeBlue(cliente.XTIPO_IDTIPO) ? 1 : 0;
                if (debeCargarPuntos)
                {
                    _fisiotes.Clientes.InsertOrUpdateBeBlue(
                    clienteDTO.Trabajador, clienteDTO.Tarjeta, cliente.IDCLIENTE, dniCliente, clienteDTO.Nombre.Strip(), clienteDTO.Telefono, clienteDTO.Direccion.Strip(),
                    clienteDTO.Movil, clienteDTO.Email, clienteDTO.Puntos, clienteDTO.FechaNacimiento, clienteDTO.Sexo, clienteDTO.FechaAlta, clienteDTO.Baja, clienteDTO.Lopd,
                    beBlue);
                }
                else
                {
                    _fisiotes.Clientes.InsertOrUpdateBeBlue(
                        clienteDTO.Trabajador, clienteDTO.Tarjeta, cliente.IDCLIENTE, dniCliente, clienteDTO.Nombre.Strip(), clienteDTO.Telefono, clienteDTO.Direccion.Strip(),
                        clienteDTO.Movil, clienteDTO.Email, clienteDTO.FechaNacimiento, clienteDTO.Sexo, clienteDTO.FechaAlta, clienteDTO.Baja, clienteDTO.Lopd,
                        beBlue);
                }
            }
            else if (debeCargarPuntos)
            {
                _fisiotes.Clientes.InsertOrUpdate(
                    clienteDTO.Trabajador, clienteDTO.Tarjeta, cliente.IDCLIENTE, dniCliente, clienteDTO.Nombre.Strip(), clienteDTO.Telefono, clienteDTO.Direccion.Strip(),
                    clienteDTO.Movil, clienteDTO.Email, clienteDTO.Puntos, clienteDTO.FechaNacimiento, clienteDTO.Sexo, clienteDTO.FechaAlta, clienteDTO.Baja, clienteDTO.Lopd,
                    withTrack: false);
            }
            else
            {
                _fisiotes.Clientes.InsertOrUpdate(
                    clienteDTO.Trabajador, clienteDTO.Tarjeta, cliente.IDCLIENTE, dniCliente, clienteDTO.Nombre.Strip(), clienteDTO.Telefono, clienteDTO.Direccion.Strip(),
                    clienteDTO.Movil, clienteDTO.Email, clienteDTO.FechaNacimiento, clienteDTO.Sexo, clienteDTO.FechaAlta, clienteDTO.Baja, clienteDTO.Lopd,
                    withTrack: false);
            }
        }
    }
}