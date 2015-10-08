﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Windows.UI.Popups;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ProjetoAthena
{
    class AthenaData
    {
        #region variaveis
        private string siteAthenaBase = "https://www.athena.biblioteca.unesp.br/F/";
        private string siteAthena = "http://www.athena.biblioteca.unesp.br/F/?func=BOR-INFO";
        private string siteAthenaLivros = "http://www.athena.biblioteca.unesp.br/F/ID_TOKEN?func=bor-loan&adm_library=UEP50";
        public string SiteAthena
        {
            get
            {
                return siteAthena;
            }
        }
        private string siteAthenaLogin = "http://www.athena.biblioteca.unesp.br/F/ID_TOKEN?func=BOR-INFO&ssl_flag=Y&func=login-session&login_source=LOGIN-BOR&bor_id=ID_USUARIO&bor_verification=ID_SENHA&bor_library=UEP50";
        public string SiteAthenaLogin
        {
            get
            {
                return siteAthenaLogin.Replace("ID_USUARIO", usuario).Replace("ID_SENHA", senha).Replace("ID_TOKEN", token);
            }
        }
        private string usuario;
        public string Usuario
        {
            get
            {
                return usuario;
            }            
            set
            {
                usuario = value;
            }
        }
        private string senha;
        public string Senha
        {
            get
            {
                return senha;
            }
            set
            {
                senha = value;
            }
        }
        private string token;
        public string Token
        {
            get
            {
                return token;
            }
            set
            {
                token = value;
            }
        }
        private bool dadosIncorretos = false;
        public bool DadosIncorretos
        {
            get
            {
                return dadosIncorretos;
            }
            set
            {
                dadosIncorretos = value;
            }
        }
        private bool usuariologado = false;
        public bool UsuarioLogado
        {
            get
            {
                return usuariologado;
            }
            set
            {
                usuariologado = value;
            }
        }
        private bool erro = false;
        public bool Erro
        {
            get
            {
                return erro;
            }
            set
            {
                erro = value;
            }
        }

        public string SiteAthenaLivros
        {
            get
            {
                return siteAthenaLivros.Replace("ID_TOKEN",Token);
            }            
        }

        private AsyncCallback callbackLogin,callbackLivros;
        private WebRequest webRequest;
        #endregion
        #region logar usuario
        private string resposta = "NADA";
        public string Resposta()
        {
            return resposta;
        }

        public void LogarUsuario(AsyncCallback callback)
        {
            usuario = "44270814870";
            senha = "2408";
            try
            {
                /*if (dadosIncorretos)
                {
                    var dialog = new MessageDialog("logar usuario 15 MINUTOS!");
                    await dialog.ShowAsync();
                    callback(null);
                }
                else
                {*/
                    callbackLogin = callback;
                    webRequest = WebRequest.Create(SiteAthena);
                    webRequest.Headers["Connection"] = "Keep-Alive";                
                    webRequest.BeginGetResponse(LogarUsuarioCallback, webRequest);
                //
            }
            catch (Exception e)
            {
                erro = true;                
                callback(null);                
            }
        }

        public void LogarUsuarioCallback(IAsyncResult resultado)
        {
            try
            {
                webRequest = resultado.AsyncState as WebRequest;
                if (webRequest != null)
                {
                    WebResponse response = (WebResponse)webRequest.EndGetResponse(resultado);
                    Stream streamResponse = response.GetResponseStream();
                    StreamReader streamReader = new StreamReader(streamResponse);
                    string responseString = streamReader.ReadToEnd();
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(responseString);
                    HtmlNode link = doc.DocumentNode.Descendants().Where(n => n.Name == "form").FirstOrDefault();
                    string tokenLogin = "";
                    if (link != null)
                    {
                        tokenLogin = link.Attributes["action"].Value;
                        tokenLogin = tokenLogin.Remove(0, tokenLogin.IndexOf("/F/") + 3);
                        Token = tokenLogin;                        
                        webRequest = WebRequest.Create(SiteAthenaLogin);
                        webRequest.Headers["Connection"] = "Keep-Alive";
                        webRequest.BeginGetResponse(LoginExecutadoCallback, webRequest);                        
                    }
                }
                else
                {
                    erro = true;                    
                    callbackLogin(null);                    
                }
            }
            catch (Exception e)
            {
                erro = true;                
                callbackLogin(null);                
            }
        }

        void LoginExecutadoCallback(IAsyncResult resultado)
        {
            try
            {
                webRequest = resultado.AsyncState as WebRequest;
                if (webRequest != null)
                {                    
                    WebResponse response = (WebResponse)webRequest.EndGetResponse(resultado);
                    Stream responseStream = response.GetResponseStream();
                    StreamReader responseStreamReader = new StreamReader(responseStream);
                    string responseString = responseStreamReader.ReadToEnd();
                    if (responseString.Contains("<!-- filename: login-session-uep01 -->"))
                    {
                        erro = true;                        
                        dadosIncorretos = true;                        
                        callbackLogin(null);
                    }
                    else
                    {
                        erro = false;
                        dadosIncorretos = false;
                        callbackLogin(resultado);                        
                    }
                }
                else
                {
                    erro = true;                    
                    callbackLogin(null);
                }
            }
            catch (Exception e)
            {                
                erro = true;               
                callbackLogin(null);                
            }
        }

        #endregion
        #region livros

        public void RetornarLivros(AsyncCallback callback)
        {
            callbackLivros = callback;
            LogarUsuario(RetornarLivrosLoginCallback);
        }

        private void RetornarLivrosLoginCallback(IAsyncResult resultado)
        {
            if (!erro)
            {
                webRequest = (WebRequest)WebRequest.Create(SiteAthenaLivros);
                webRequest.Headers["Connection"] = "Keep-Alive";
                webRequest.BeginGetResponse(RetornarLivrosCarregarCallback,webRequest);
            }
            else
            {
                callbackLivros(null);
            }
        }
        #endregion
    }
}
