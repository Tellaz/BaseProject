namespace BaseProject.DAO.Models.Others
{
    public class EmailOptions
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Template { get; set; }
        public List<KeyValuePair<string, string>> PlaceHolders { get; set; }
    }

    public static class EmailTemplate
	{
        /// <summary>
        /// Template para autenticação de dois fatores
        /// </summary>
        public static string CodigoSeguranca { get; } = nameof(CodigoSeguranca);

        /// <summary>
        /// Template para confirmação do email do usuário
        /// </summary>
        public static string ConfirmarEmail { get; } = nameof(ConfirmarEmail);

        /// <summary>
        /// Template com os dados de acesso de um novo usuário
        /// </summary>
        public static string NovoUsuario { get; } = nameof(NovoUsuario);

        /// <summary>
        /// Template para redefinir a senha do usuário
        /// </summary>
        public static string RedefinirSenha { get; } = nameof(RedefinirSenha);
		/// <summary>
		/// Template para notificar os aprovadores de uma nova descrição a aprovar
		/// </summary>
		public static string NotificacaoAprovador { get; } = nameof(NotificacaoAprovador);
        /// <summary>
		/// Template para notificar os participantes das pesquisas
		/// </summary>
		public static string NotificarParticipantePesquisa { get; } = nameof(NotificarParticipantePesquisa);
        /// <summary>
		/// Template para confirmar a adesão de um cliente para uma consultoria de descrição de cargos
		/// </summary>
		public static string ConfirmarClienteConsultoriaDescricao { get; } = nameof(ConfirmarClienteConsultoriaDescricao);
    }
}
