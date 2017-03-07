using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Irony.Parsing;
using SBScript.Interprete;
using SBScript.Interprete.expresiones;
using System.IO;

namespace SBScript
{
    public partial class Form1 : Form
    {
        interprete interprete = new interprete();
        Stack<nodo_expresion> pila_expresion = new Stack<nodo_expresion>();
        String pars = "";
        public Form1()
        {
            InitializeComponent();
        }

        private void btnAnalizar_Click(object sender, EventArgs e)
        {
            analizar();
        }

        private void analizar(){
            @const.bandera_incerteza = true;
            limpiar_pictures();
            Consola.AppendText("\r\n--------------------------------------------Nueva ejecución: --------------------------------------------");
            errores.errores.clear();
            interprete = new interprete();
            @const.interprete = interprete;

            //-------------extraccion de la entrada-----------
            String entrada = "";
            TextBox txt = (TextBox)contenedor.SelectedTab.Controls[0];
            entrada = txt.Text;
            //------------------------------------------------

            int respuesta = analizar(entrada);
            //interprete.show();
            if (respuesta > 0)
            {
                interprete.ejecutar();
                errores.errores.show();
            }
        }

        private int analizar(String entrada) {
            Analizador a = new Analizador();
            return parser(a, entrada);
        }

        public int parser(Grammar gramatica, String entrada){
            //analiza 
            LanguageData lenguaje = new LanguageData(gramatica);
            Parser parser = new Parser(lenguaje);
            
            ParseTree arbol = parser.Parse(entrada);
            ParseTreeNode raiz = arbol.Root;

            if (arbol.ParserMessages.Count>0) {
                MessageBox.Show("Se encontraron errores durante el analisis, dirigase a la seccion de errores para corregirlos");
                foreach (Irony.LogMessage error in arbol.ParserMessages)
                {
                    try
                    {
                        if (error.Message.Contains("Invalid"))
                        {
                            String tokens = error.Message.Split('\'')[1];
                            errores.errores.add_error("Error Lexico", "Caracter Invalido: " + tokens, error.Location.Line, error.Location.Column);
                        }
                        else
                        {
                            String tokens = error.Message.Split(':')[1];
                            errores.errores.add_error("Error Sintactico", "se esperaban los tokens: " + tokens, error.Location.Line, error.Location.Column);
                        }
                    }
                    catch (Exception e) {
                    }
                }
            }

            if (raiz == null) { 
                MessageBox.Show("Se encontraron errores durante el analisis de los cuales no pudimos recuperarnos :/");
                return -1; 
            }

            //crea un nuevo archivo para almacenar datos
            interprete.archivo_nuevo();
            recorrer_arbol(raiz);
            interprete.pop_archivo();
            return 1;
        }

        void recorrer_arbol(ParseTreeNode raiz) {
            
            String termino="",token = "";
            if (raiz.Term != null)
                termino = raiz.Term.ToString();
            if (raiz.Token != null)
                token = raiz.Token.ToString();
            if (termino.Equals("incerteza"))
            {
                String numero = raiz.ChildNodes.ElementAt(1).Token.Text;
                Double incerteza = Double.Parse(numero.Replace('.', ','));
                this.interprete.set_incerteza(incerteza);
            }
            else if (termino.Equals("ruta"))
            {
                String ruta = raiz.ChildNodes.ElementAt(1).Token.Text;
                this.interprete.set_ruta(ruta);
            }
            else if (termino.Equals("incluye"))
            {
                String texto = "";
                String incluye = raiz.ChildNodes.ElementAt(1).Token.Text;
                try
                {
                    String ruta = contenedor.SelectedTab.Name;
                    String[] partes = ruta.Split('\\');
                    ruta = "";
                    for (int i = 0; i < partes.Length - 1; i++) {
                        ruta += partes[i]+"\\";
                    }
                    System.IO.StreamReader file = new System.IO.StreamReader(ruta+incluye);
                    texto = file.ReadToEnd();
                    file.Close();

                    analizar(texto);
                }
                catch (Exception e) {
                    errores.errores.add_error("Error Semantico", "Archivo " + incluye + " inaccesible", raiz.FindToken().Location.Line, raiz.FindToken().Location.Column);
                    return;
                }


                /*
                 * String incluye = raiz.ChildNodes.ElementAt(1).Token.Text;
                 * Console.WriteLine("debería de incluir " + incluye);
                 * analizar("Define 666 Define \"nuevo\" Principal () { alo= polisia; }");
                */
            }
            else if (termino.Equals("declaracion"))
            {
                String tipo = raiz.ChildNodes.ElementAt(0).Token.Text;
                List<String> ids = new List<string>();
                for (int i = 0; i < raiz.ChildNodes.ElementAt(1).ChildNodes.Count; i++)
                {
                    ids.Add(raiz.ChildNodes.ElementAt(1).ChildNodes.ElementAt(i).Token.Text);
                }
                arbol_expresion arbol = new arbol_expresion();

                if (raiz.ChildNodes.ElementAt(2).ChildNodes.Count > 0)
                {
                    arbol.raiz = extraer_arbol(raiz.ChildNodes.ElementAt(2).ChildNodes.ElementAt(1));
                    //@const.dibujar_expresion(arbol.raiz, "prueba_expresion");
                    /*nodo_expresion resultado= arbol.ejecutar_arbol();
                    Console.WriteLine("----------------------");
                    Console.WriteLine(resultado.valor + " " + resultado.tipo + " " + resultado.rol);
                    Console.WriteLine("----------------------");
                    */
                }

                interprete.add_sentencia("declarar", ids, arbol, tipo.ToLower(), raiz.FindToken().Location.Line, raiz.FindToken().Location.Column);
            }
            else if (termino.Equals("declarar_funcion"))
            {
                if (raiz.ChildNodes.ElementAt(0).Token.Text.Equals("Principal"))
                {
                    interprete.funcion_nueva("Principal", interprete.get_funcion(), "void");
                    raiz.ChildNodes.ForEach(recorrer_arbol);
                    interprete.set_principal();
                    return;
                }
                else
                {
                    interprete.funcion_nueva(raiz.ChildNodes.ElementAt(1).Token.Text, interprete.get_funcion(), raiz.ChildNodes.ElementAt(0).Token.Text);
                    raiz.ChildNodes.ForEach(recorrer_arbol);
                    interprete.agregar_nombre(pars);
                    pars = "";
                    interprete.pop_funcion();
                    return;
                }
            }
            else if (termino.Equals("parametro"))
            {
                String tipo = raiz.ChildNodes.ElementAt(0).Token.Text;
                String nombre = raiz.ChildNodes.ElementAt(1).Token.Text;
                interprete.add_parametro(tipo, nombre);
                pars += "#" + tipo.ToLower();
            }
            else if (termino.Equals("asignacion"))
            {
                String id = raiz.ChildNodes.ElementAt(0).Token.Text;
                List<String> lista = new List<string>();
                lista.Add(id);
                nodo_expresion expresion = extraer_arbol(raiz.ChildNodes.ElementAt(2));
                arbol_expresion arbol = new arbol_expresion();
                arbol.raiz = expresion;
                interprete.add_sentencia("asignacion", lista, arbol, raiz.FindToken().Location.Line, raiz.FindToken().Location.Column);
            }
            else if (termino.Equals("retorno_noterminal"))
            {
                arbol_expresion arbol = new arbol_expresion();
                if (raiz.ChildNodes.Count > 1)
                {
                    arbol.raiz = extraer_arbol(raiz.ChildNodes.ElementAt(1));
                }
                interprete.add_sentencia("retorno", null, arbol, raiz.FindToken().Location.Line, raiz.FindToken().Location.Column);
            }
            else if (termino.Equals("sentencia_continuar"))
            {
                interprete.add_sentencia("continuar", null, new arbol_expresion(), raiz.FindToken().Location.Line, raiz.FindToken().Location.Column);
            }
            else if (termino.Equals("sentencia_detener"))
            {
                interprete.add_sentencia("detener", null, new arbol_expresion(), raiz.FindToken().Location.Line, raiz.FindToken().Location.Column);
            }
            else if (termino.Equals("call_funcion"))
            {
                arbol_expresion arbol = new arbol_expresion();
                arbol.raiz = extraer_arbol(raiz.ChildNodes.ElementAt(0));
                interprete.add_sentencia("call_funcion", null, arbol, raiz.FindToken().Location.Line, raiz.FindToken().Location.Column);

            }
            else if (termino.Equals("flujo_si"))
            {
                List<camino> caminos = new List<camino>();
                //extrae la condicion
                nodo_expresion cond = extraer_arbol(raiz.ChildNodes.ElementAt(0));

                //inserta funcion para anidar
                interprete.funcion_nueva("si", interprete.get_funcion(), "void");
                //inserta sentencias
                recorrer_arbol(raiz.ChildNodes.ElementAt(1));
                //obtiene el camino si 
                funcion si = interprete.extraer_funcion();
                //agrega primer camino
                caminos.Add(new camino(cond, si));

                //obtener el else
                if (raiz.ChildNodes.ElementAt(2).ChildNodes.Count > 0)
                {
                    //si hay else 
                    interprete.funcion_nueva("Sino", interprete.get_funcion(), "void");
                    recorrer_arbol(raiz.ChildNodes.ElementAt(2).ChildNodes.ElementAt(0));
                    funcion sino = interprete.extraer_funcion();
                    caminos.Add(new camino(null, sino));
                }
                interprete.add_sentencia("si", new arbol_expresion(), caminos, raiz.FindToken().Location.Line, raiz.FindToken().Location.Column);
                return;
            }
            else if (termino.Equals("flujo_selecciona"))
            {
                List<camino> caminos = new List<camino>();
                //extrae la expresion a comparar
                nodo_expresion expresion = extraer_arbol(raiz.ChildNodes.ElementAt(0));
                ParseTreeNode pares = raiz.ChildNodes.ElementAt(1);
                int caminos_cant = pares.ChildNodes.Count;
                for (int i = 0; i < caminos_cant; i++)
                {
                    ParseTreeNode par = pares.ChildNodes.ElementAt(i);
                    //hacer condicion
                    nodo_expresion condicion = new nodo_expresion("==", "operador", "operador",raiz.FindToken().Location.Line,raiz.FindToken().Location.Column);
                    condicion.izq = expresion;
                    //extraer valor
                    nodo_expresion valor = extraer_arbol(par.ChildNodes.ElementAt(0));
                    condicion.der = valor;

                    //extraer el camino
                    interprete.funcion_nueva("camino", interprete.get_funcion(), "void");
                    recorrer_arbol(par.ChildNodes.ElementAt(1));
                    funcion flujo = interprete.extraer_funcion();
                    //insertar el camino
                    caminos.Add(new camino(condicion, flujo));
                }
                //obtener el Defecto
                if (raiz.ChildNodes.ElementAt(2).ChildNodes.Count > 0)
                {
                    //si hay defecto
                    interprete.funcion_nueva("defecto", interprete.get_funcion(), "void");
                    recorrer_arbol(raiz.ChildNodes.ElementAt(2).ChildNodes.ElementAt(0));
                    funcion defecto = interprete.extraer_funcion();
                    caminos.Add(new camino(null, defecto));
                }
                interprete.add_sentencia("selecciona", new arbol_expresion(), caminos, raiz.FindToken().Location.Line, raiz.FindToken().Location.Column);
                return;
            }
            else if (termino.Equals("flujo_para"))
            {
                //crear sentencia de asignacion inicial
                
                List<String> id = new List<string>();
                id.Add(raiz.ChildNodes.ElementAt(1).Token.Text);
                arbol_expresion expresion = new arbol_expresion();
                expresion.raiz = extraer_arbol(raiz.ChildNodes.ElementAt(3));
                sentencia asignacion_inicial= new sentencia("declarar", id, expresion, raiz.FindToken().Location.Line, raiz.FindToken().Location.Column);
                asignacion_inicial.tipo_dato = "number";
                //extraer condicion
                nodo_expresion condicion = extraer_arbol(raiz.ChildNodes.ElementAt(4));

                //sacar flujo
                List<camino> caminos = new List<camino>();
                //inserta funcion para anidar
                interprete.funcion_nueva("para", interprete.get_funcion(), "void");
                //inserta sentencias
                recorrer_arbol(raiz.ChildNodes.ElementAt(6));
                //obtiene el camino 
                funcion para = interprete.extraer_funcion();

                //meter paso al flujo
                nodo_expresion op = new nodo_expresion("","","",-1,-1);
                nodo_expresion var = new nodo_expresion(id.ElementAt(0), "terminal", "id", raiz.FindToken().Location.Line, raiz.FindToken().Location.Column);
                nodo_expresion uno = new nodo_expresion("1", "terminal", "NUMERO", raiz.FindToken().Location.Line, raiz.FindToken().Location.Column);

                if (raiz.ChildNodes.ElementAt(5).Token.Text.Equals("++"))
                {
                    op = new nodo_expresion("+", "operador", "operador", raiz.FindToken().Location.Line, raiz.FindToken().Location.Column);
                }
                else if (raiz.ChildNodes.ElementAt(5).Token.Text.Equals("--")) {
                    op = new nodo_expresion("-", "operador", "operador", raiz.FindToken().Location.Line, raiz.FindToken().Location.Column);
                }

                op.izq = var;
                op.der = uno;

                arbol_expresion asignacion = new arbol_expresion();
                asignacion.raiz = op;
                sentencia paso = new sentencia("asignacion", id, asignacion, raiz.FindToken().Location.Line, raiz.FindToken().Location.Column);

                para.add_sentencia(paso);

                //agrega primer camino
                caminos.Add(new camino(condicion, para));



                //meter sentencia 
                interprete.add_sentencia("para", asignacion_inicial, caminos, raiz.FindToken().Location.Line, raiz.FindToken().Location.Column);
                return;
            }
            else if (termino.Equals("flujo_hasta"))
            {
                //extraer condicion
                nodo_expresion condicion = extraer_arbol(raiz.ChildNodes.ElementAt(0));
                //sacar flujo
                List<camino> caminos = new List<camino>();
                //inserta funcion para anidar
                interprete.funcion_nueva("hasta", interprete.get_funcion(), "void");
                //inserta sentencias
                recorrer_arbol(raiz.ChildNodes.ElementAt(1));
                //obtiene el camino 
                funcion hasta = interprete.extraer_funcion();

                //agrega primer camino
                caminos.Add(new camino(condicion, hasta));
                //meter sentencia 
                interprete.add_sentencia("hasta", new arbol_expresion(), caminos, raiz.FindToken().Location.Line, raiz.FindToken().Location.Column);
                return;
            }
            else if (termino.Equals("flujo_mientras"))
            {
                //extraer condicion
                nodo_expresion condicion = extraer_arbol(raiz.ChildNodes.ElementAt(0));
                //sacar flujo
                List<camino> caminos = new List<camino>();
                //inserta funcion para anidar
                interprete.funcion_nueva("mientras", interprete.get_funcion(), "void");
                //inserta sentencias
                recorrer_arbol(raiz.ChildNodes.ElementAt(1));
                //obtiene el camino 
                funcion mientras = interprete.extraer_funcion();

                //agrega primer camino
                caminos.Add(new camino(condicion, mientras));
                //meter sentencia 
                interprete.add_sentencia("mientras", new arbol_expresion(), caminos, raiz.FindToken().Location.Line, raiz.FindToken().Location.Column);
                return;
            }
            //Console.WriteLine("-"+termino+"-"+ token+"-");

            raiz.ChildNodes.ForEach(recorrer_arbol);
        }

        public nodo_expresion extraer_arbol(ParseTreeNode raiz) {
            String termino = "", token = "";
            if (raiz.Term != null)
                termino = raiz.Term.ToString();
            if (raiz.Token != null)
                token = raiz.Token.Text;

            if (termino.Equals("NUMERO") || termino.Equals("CADENA") || termino.Equals("True") || termino.Equals("False") || termino.Equals("ID"))
            {
                String tipo = "";
                if (termino.Equals("NUMERO"))
                {
                    tipo = "number";
                }
                else if (termino.Equals("CADENA"))
                {
                    tipo = "string";
                }
                else if (termino.Equals("True")) {
                    token = "1";
                    tipo = "bool";
                }
                else if (termino.Equals("False"))
                {
                    token = "0";
                    tipo = "bool";
                }
                else
                {
                    tipo = "id";
                }
                nodo_expresion nuevo = new nodo_expresion(token.Replace("\"",""), "terminal", tipo,raiz.Token.Location.Line,raiz.Token.Location.Column);
                return nuevo;
            }
            else if (termino.Equals("llamar_funcion"))
            {
                nodo_expresion funcion = new nodo_expresion(raiz.ChildNodes.ElementAt(0).Token.Text, "llamar_funcion", "llamar_funcion", raiz.ChildNodes.ElementAt(0).Token.Location.Line, raiz.ChildNodes.ElementAt(0).Token.Location.Column);
                List<arbol_expresion> expresiones = new List<arbol_expresion>();
                ParseTreeNode lista = raiz.ChildNodes.ElementAt(1);
                for (int i = 0; i < lista.ChildNodes.Count; i++)
                {
                    arbol_expresion arbol = new arbol_expresion();
                    arbol.raiz = extraer_arbol(lista.ChildNodes.ElementAt(i));
                    expresiones.Add(arbol);
                }
                funcion.parametros = expresiones;
                //METER PARAMETROS AL NODO
                return funcion;
            }
            else if (termino.Equals("exprecion"))
            {
                if (raiz.ChildNodes.Count == 3)
                {
                    nodo_expresion izq = extraer_arbol(raiz.ChildNodes.ElementAt(0));
                    nodo_expresion der = extraer_arbol(raiz.ChildNodes.ElementAt(2));
                    nodo_expresion op = extraer_arbol(raiz.ChildNodes.ElementAt(1));
                    op.izq = izq;
                    op.der = der;
                    return op;
                }
                else if (raiz.ChildNodes.Count == 2)
                {
                    nodo_expresion izq = extraer_arbol(raiz.ChildNodes.ElementAt(1));
                    nodo_expresion op = extraer_arbol(raiz.ChildNodes.ElementAt(0));
                    op.izq = izq;
                    return op;
                }
                else
                {
                    nodo_expresion nodo = extraer_arbol(raiz.ChildNodes.ElementAt(0));
                    return nodo;
                }
            }
            else if (termino.Equals("relacion"))
            {
                nodo_expresion izq = extraer_arbol(raiz.ChildNodes.ElementAt(0));
                nodo_expresion der = extraer_arbol(raiz.ChildNodes.ElementAt(2));
                nodo_expresion op = extraer_arbol(raiz.ChildNodes.ElementAt(1));
                op.izq = izq;
                op.der = der;
                return op;
            }
            else if (termino.Equals("condicion")) {
                if (raiz.ChildNodes.Count == 3)
                {
                    nodo_expresion izq = extraer_arbol(raiz.ChildNodes.ElementAt(0));
                    nodo_expresion der = extraer_arbol(raiz.ChildNodes.ElementAt(2));
                    nodo_expresion op = extraer_arbol(raiz.ChildNodes.ElementAt(1));
                    op.izq = izq;
                    op.der = der;
                    return op;
                }
                else if (raiz.ChildNodes.Count == 2)
                {
                    nodo_expresion izq = extraer_arbol(raiz.ChildNodes.ElementAt(1));
                    nodo_expresion op = extraer_arbol(raiz.ChildNodes.ElementAt(0));
                    op.izq = izq;
                    return op;
                }
                else
                {
                    nodo_expresion nodo = extraer_arbol(raiz.ChildNodes.ElementAt(0));
                    return nodo;
                }
            }
            else if (!termino.Equals("exprecion"))
            {
                nodo_expresion nuevo;
                try
                {
                    nuevo = new nodo_expresion(token, "operador", "operador", raiz.FindToken().Location.Line, raiz.FindToken().Location.Column);
                }
                catch (Exception e) {
                    nuevo = new nodo_expresion(token, "operador", "operador", -1,-1);
                }
                return nuevo;
            }
            

            return null;

            
            //Console.WriteLine(termino + " , " + token);
        }

        public TextBox get_consola() {
            return Consola;
        }

        private void nuevoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ARCHIVO NUEVO
            TabPage pestaña = new TabPage("nuevo");
            TextBox texto = new TextBox();
            texto.AcceptsTab = true;
            texto.Multiline = true;
            texto.Name = "entrada";
            texto.RightToLeft = System.Windows.Forms.RightToLeft.No;
            texto.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            texto.Size = new Size(500,400);
            pestaña.Controls.Add(texto);
            contenedor.TabPages.Add(pestaña);
            contenedor.SelectTab(contenedor.TabCount-1);

        }

        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ABRIR ARCHIVO
            openFileDialog1.Filter = "Archivos SBScript|*.sbs";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.StreamReader sr = new
                   System.IO.StreamReader(openFileDialog1.FileName);
                String cadena = sr.ReadToEnd();

                //AGREGANDO TAB Y TEXTO
                TabPage pestaña = new TabPage(openFileDialog1.SafeFileName);
                pestaña.Name = openFileDialog1.FileName;
                TextBox texto = new TextBox();
                texto.AcceptsTab = true;
                texto.Multiline = true;
                texto.Name = openFileDialog1.FileName;
                texto.RightToLeft = System.Windows.Forms.RightToLeft.No;
                texto.ScrollBars = System.Windows.Forms.ScrollBars.Both;
                texto.Size = new Size(500, 400);
                texto.Text = cadena;
                pestaña.Controls.Add(texto);
                contenedor.TabPages.Add(pestaña);
                contenedor.SelectTab(contenedor.TabCount - 1);
                //--------------------

                sr.Close();
            }
        }

        private void guardarComoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //GUARDAR COMO
            if (contenedor.TabCount > 0) { 
                String texto = "";
                TextBox tbTexto = (TextBox)contenedor.SelectedTab.Controls[0];
                texto = tbTexto.Text;
                //MessageBox.Show(texto);
                saveFileDialog1.Filter = "Archivos SBScript|*.sbs";
                saveFileDialog1.ShowDialog();
                if (saveFileDialog1.FileName != "")
                {
                    using (StreamWriter sw = new StreamWriter(saveFileDialog1.OpenFile()))
                    {
                        sw.Write(texto);
                    }
                    tbTexto.Name = saveFileDialog1.FileName;
                    String[] partes = saveFileDialog1.FileName.Split('\\');
                    String nombre = partes[partes.Length-1];
                    contenedor.SelectedTab.Text=nombre;
                    contenedor.SelectedTab.Name = saveFileDialog1.FileName;
                }
            }
        }

        private void guardarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //GUARDAR
            if (contenedor.TabCount > 0)
            {
                String texto = "";
                TextBox tbTexto = (TextBox)contenedor.SelectedTab.Controls[0];
                texto = tbTexto.Text;

                System.IO.StreamWriter file = new System.IO.StreamWriter(tbTexto.Name);
                file.WriteLine(texto);
                file.Close();
            }
        }

        private void cerrarPestañaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //CERRAR
            int indice=contenedor.SelectedIndex;
            contenedor.Controls.RemoveAt(indice);
        }

        private void ejecutarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            analizar();
        }

        private void actualizar_galeria(object sender, EventArgs e)
        {
            try
            {
                String ruta = @const.RUTA.Replace("\"", "");
                ruta = ruta.Replace("/", "\\\\") + "\\";
                DirectoryInfo di = new DirectoryInfo(ruta);
                int i = 0;
                foreach (FileInfo file in di.GetFiles("*.png"))
                {
                    if (i == 0)
                    {
                        System.IO.FileStream fs = new System.IO.FileStream(ruta+file.Name, FileMode.Open, FileAccess.Read);
                        pictureBox1.Image = System.Drawing.Image.FromStream(fs);
                        fs.Close();

                        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                        i++;
                    }
                    else if (i == 1)
                    {
                        System.IO.FileStream fs = new System.IO.FileStream(ruta + file.Name, FileMode.Open, FileAccess.Read);
                        pictureBox2.Image = System.Drawing.Image.FromStream(fs);
                        fs.Close();

                        pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                        i++;
                    }
                    else if (i == 2)
                    {
                        System.IO.FileStream fs = new System.IO.FileStream(ruta + file.Name, FileMode.Open, FileAccess.Read);
                        pictureBox3.Image = System.Drawing.Image.FromStream(fs);
                        fs.Close();

                        pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
                        i++;
                    }
                    else if (i == 3)
                    {
                        System.IO.FileStream fs = new System.IO.FileStream(ruta + file.Name, FileMode.Open, FileAccess.Read);
                        pictureBox4.Image = System.Drawing.Image.FromStream(fs);
                        fs.Close();

                        pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
                        i++;
                    }
                    else if (i == 4)
                    {
                        System.IO.FileStream fs = new System.IO.FileStream(ruta + file.Name, FileMode.Open, FileAccess.Read);
                        pictureBox5.Image = System.Drawing.Image.FromStream(fs);
                        fs.Close();

                        pictureBox5.SizeMode = PictureBoxSizeMode.StretchImage;
                        i = 0;
                    }
                }
            }
            catch (Exception ex) {

            }
            
        }

        private void limpiar_pictures() {
            pictureBox1.Image = null;
            pictureBox2.Image = null;
            pictureBox3.Image = null;
            pictureBox4.Image = null;
            pictureBox5.Image = null;
        }

        private void abrirAlbumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String ruta = @const.RUTA.Replace("\"", "");
            ruta = ruta.Replace("/", "\\\\") + "\\";
            System.Diagnostics.Process.Start(ruta);
        }

        private void verErroresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            @const.reporte_errores();
        }
    }
}
