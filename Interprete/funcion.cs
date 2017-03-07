using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBScript.Interprete
{
    public class funcion
    {
        public String nombre, tipo;
        public List<sentencia> sentencias = new List<sentencia>();
        public variables variables = new variables();
        public List<String> parametros = new List<string>();
        public funcion padre=null;
        

        public funcion(String nombre,funcion padre,String tipo) {
            this.nombre = nombre;
            this.padre = padre;
            this.tipo = tipo;
        }

        public void add_sentencia(sentencia sentencia) {
            sentencias.Add(sentencia);
        }

        public void add_parametro(String tipo, String nombre) {
            parametros.Add(tipo + ","+nombre);
        }

        public void agregar_nombre(String s) {
            nombre += s;
        }
    }
}
