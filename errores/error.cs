using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBScript.errores
{
    public class error
    {
        public String tipo, descripcion;
        public int fila, columna;
        public error(String tipo,String descripcion, int fila, int columna) {
            this.tipo = tipo;
            this.descripcion = descripcion;
            this.fila = fila;
            this.columna = columna;
        }

        public void show() {
            Console.WriteLine(tipo + " " + descripcion + " fila: " + fila + " columna: " + columna);
        }
    }
}
