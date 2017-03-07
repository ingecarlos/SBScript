using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBScript.Interprete
{
    public class variable
    {
        public String nombre, valor,tipo;
        public variable(String tipo,String nombre, String valor) {
            this.nombre = nombre;
            this.valor = valor;
            this.tipo = tipo;
        }
    }
}
