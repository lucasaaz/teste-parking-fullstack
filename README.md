### Stack de Referência
- **Backend**: .NET 8 Web API + EF Core + PostgreSQL  
- **Frontend**: React (Vite) + React Router + React Query  
- **Sem containers**: a conexão é configurada diretamente em `appsettings.json`.  

> Foi mantido o React e o ORM.  

### Execução Local

#### Banco PostgreSQL (pgAdmin 4)
Banco local chamado parking_test utilizando o pgAdmin 4.

Ajuste a ConnectionString em appsettings.json com suas credenciais do PostgreSQL. Minha senha foi "123456".

Execute o script de seed (scripts/seed.sql) através da ferramenta Query Tool do pgAdmin para popular as tabelas iniciais.

#### Backend
```bash
cd src/backend
dotnet restore
dotnet run
```
A API será iniciada (por padrão) em `http://localhost:5000`. Swagger ativado em `/swagger`.  

#### Frontend
```bash
cd src/frontend
npm install
npm run dev
```
A aplicação ficará disponível em `http://localhost:5173`.  
Configure `VITE_API_URL` caso seja necessário apontar para outra porta.  

### 4.3 Estrutura de Pastas
```
/src/backend        -> API .NET 8
/src/frontend       -> React (Vite)
/scripts/seed.sql   -> Criação e seed do banco
/scripts/exemplo.csv-> CSV de exemplo
```

### 🚀 Decisões Técnicas e Implementações
1. Gestão de Clientes e Unicidade

Edição Completa: Implementada a funcionalidade de edição de clientes (Nome, Telefone, Endereço, Mensalidade e Status).

Integridade de Dados: Foi adicionada uma Unique Constraint composta para os campos Nome + Telefone no PostgreSQL via AppDbContext, garantindo que não existam registros duplicados "na raiz" do sistema.

Feedback de Erro: No backend, a API valida a duplicidade e retorna um 400 BadRequest com mensagem personalizada, que é capturada pelo frontend e exibida ao usuário.

2. Gestão de Veículos e Relacionamentos
Troca de Proprietário: O formulário de veículos foi refatorado para permitir a alteração do cliente associado através de um seletor dinâmico, garantindo a integridade referencial.

Suporte a Placas Mercosul: O PlacaService foi atualizado para validar tanto o formato antigo (AAA1234) quanto o novo padrão Mercosul (AAA1A23).

3. Importação Resiliente de CSV

Observabilidade: A rotina de importação foi alterada para não interromper o processo em caso de erro em uma linha específica.

Relatório Detalhado: Implementado um relatório de processamento que indica o total de sucessos e uma lista detalhada de falhas, especificando a linha exata e o motivo do erro.

4. Faturamento Parcial (Proporcional)

Lógica de Cobrança Justa: Corrigido o bug que utilizava o valor cheio da mensalidade independente da data de entrada do veículo.

Cálculo por Dias Ativos: O FaturamentoService agora calcula o valor diário com base nos dias reais do mês (DateTime.DaysInMonth) e multiplica pela quantidade de dias em que o veículo esteve sob posse do cliente na competência atual.


Exemplo: Um veículo incluído no dia 11/09 em uma mensalidade de R$ 300,00 gera uma fatura automática de R$ 200,00 (correspondente aos 20 dias ativos do mês).

