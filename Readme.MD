# Nordware

Desafio técnico para uma plataforma de e-commerce que permite a reserva de produtos, com controle de concorrência e persistência utilizando Entity Framework Core.

## Pré-requisitos

Antes de executar o projeto, certifique-se de possuir:

* .NET SDK 8.0 ou superior
* Git

Verifique a instalação:

```bash
dotnet --version
git --version
```

## Clonando o repositório

```bash
git clone <URL_DO_REPOSITORIO>
cd Nordware
```

## Restaurando as dependências

```bash
dotnet restore
```

## Compilando a solução

```bash
dotnet build
```

## Executando os testes

Para executar todos os testes:

```bash
dotnet test
```

Para executar os testes com informações detalhadas:

```bash
dotnet test --verbosity normal
```

## Testes de concorrência

Os testes de concorrência utilizam o provider:

```text
Microsoft.EntityFrameworkCore.InMemory
```

O objetivo desses testes é validar o comportamento dos diferentes `DbContext`, incluindo:

* carregamento independente da mesma entidade;
* isolamento do estado rastreado por cada contexto;
* persistência da alteração realizada por um contexto;
* leitura do valor atualizado por um novo contexto;
* manutenção do snapshot original pelo contexto que já havia carregado a entidade.

### Observação sobre o provider InMemory

O provider `EntityFrameworkCore.InMemory` é utilizado nos testes conforme solicitado.

Entretanto, o provider InMemory não deve ser utilizado como evidência de que o mecanismo de concorrência otimista de um banco relacional real lançará `DbUpdateConcurrencyException`.

Por esse motivo, os testes que utilizam InMemory validam o comportamento de tracking, snapshots e persistência compartilhada da base em memória.

O tratamento da exceção `DbUpdateConcurrencyException` pelo `UnitOfWork` deve ser testado de forma isolada.

## Estrutura da solução

### Domain

Contém as entidades e regras de domínio da aplicação.

### Application

Contém os casos de uso, comandos, handlers, contratos e serviços da aplicação.

### Infrastructure

Contém a implementação de persistência utilizando Entity Framework Core.

### API

Contém os endpoints HTTP da aplicação.

### Tests

Contém os testes automatizados da solução.

Os testes utilizam:

* xUnit
* FluentAssertions
* NSubstitute
* EntityFrameworkCore.InMemory

## Executando a API

A partir da raiz do projeto:

```bash
dotnet run --project src/Nordware.Api
```

A API ficará disponível no endereço apresentado pelo ASP.NET Core no terminal.

Caso o projeto utilize Swagger, a documentação poderá ser acessada pelo endpoint `/swagger`.

## Executando um teste específico

Para executar somente os testes de concorrência:

```bash
dotnet test --filter "FullyQualifiedName~Concurrency"
```

Para executar somente os testes do `UnitOfWork`:

```bash
dotnet test --filter "FullyQualifiedName~UnitOfWork"
```

## Build completo

O fluxo recomendado para validar o projeto antes da entrega é:

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/Nordware.Api
```

## Decisões técnicas

### Concorrência

A entidade `Product` possui uma propriedade configurada como token de concorrência no Entity Framework Core.

Exemplo:

```csharp
builder.Property(p => p.Status)
    .HasConversion<string>()
    .IsRequired()
    .IsConcurrencyToken();
```

Essa configuração representa a intenção de utilizar concorrência otimista na persistência.

### UnitOfWork

O `UnitOfWork` centraliza a chamada ao `SaveChangesAsync` e trata `DbUpdateConcurrencyException` de acordo com o contrato definido pela aplicação.

### Testes

Os testes foram divididos de acordo com a responsabilidade:

* testes de domínio para regras da entidade;
* testes de aplicação para casos de uso;
* testes de persistência para o comportamento do EF Core;
* testes de concorrência para cenários envolvendo múltiplos `DbContext`;
* testes do `UnitOfWork` para o tratamento de falhas de persistência.

## Critérios de validação

A solução deve ser considerada executável quando os comandos abaixo forem concluídos sem erro:

```bash
dotnet restore
dotnet build
dotnet test
```

## Tecnologias

* .NET
* ASP.NET Core
* Entity Framework Core
* EntityFrameworkCore.InMemory
* MediatR
* xUnit
* FluentAssertions
* NSubstitute
