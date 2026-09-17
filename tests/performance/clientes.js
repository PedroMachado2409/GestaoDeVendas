import http from 'k6/http';
import { check, fail, sleep } from 'k6';
import exec from 'k6/execution';

const BASE_URL = (__ENV.K6_BASE_URL || 'http://localhost:5143').replace(/\/$/, '');
const CENARIO = (__ENV.K6_CENARIO || 'listagem').toLowerCase();
const VUS = inteiroPositivo(__ENV.K6_VUS, 5, 'K6_VUS');
const DURACAO = __ENV.K6_DURACAO || '30s';
const PAUSA_SEGUNDOS = numeroNaoNegativo(
  __ENV.K6_PAUSA_SEGUNDOS,
  1,
  'K6_PAUSA_SEGUNDOS',
);
const P95_MS = inteiroPositivo(__ENV.K6_P95_MS, 500, 'K6_P95_MS');

const cenarios = {
  listagem: {
    exec: 'testarListagem',
    operacoes: ['listar'],
  },
  cadastro: {
    exec: 'testarCadastro',
    operacoes: ['cadastrar'],
  },
  fluxo: {
    exec: 'testarFluxo',
    operacoes: ['cadastrar', 'obter_por_id', 'atualizar', 'listar'],
  },
  'fluxo-admin': {
    exec: 'testarFluxoAdmin',
    operacoes: [
      'cadastrar',
      'obter_por_id',
      'atualizar',
      'inativar',
      'ativar',
      'listar',
    ],
  },
};

if (!cenarios[CENARIO]) {
  throw new Error(
    `K6_CENARIO invalido: "${CENARIO}". Use listagem, cadastro, fluxo ou fluxo-admin.`,
  );
}

const thresholds = {
  checks: ['rate>0.99'],
  http_req_failed: ['rate<0.01'],
};

for (const operacao of cenarios[CENARIO].operacoes) {
  thresholds[`http_req_duration{operacao:${operacao}}`] = [`p(95)<${P95_MS}`];
  thresholds[`http_req_failed{operacao:${operacao}}`] = ['rate<0.01'];
}

export const options = {
  scenarios: {
    clientes: {
      executor: 'ramping-vus',
      exec: cenarios[CENARIO].exec,
      startVUs: 1,
      stages: [
        { duration: '10s', target: VUS },
        { duration: DURACAO, target: VUS },
        { duration: '10s', target: 0 },
      ],
      gracefulRampDown: '10s',
    },
  },
  thresholds,
};

export function setup() {
  const email = __ENV.K6_EMAIL;
  const senha = __ENV.K6_SENHA;

  if (!email || !senha) {
    fail('Defina K6_EMAIL e K6_SENHA com as credenciais do usuario de teste.');
  }

  const resposta = http.post(
    `${BASE_URL}/api/Usuario/autenticar`,
    JSON.stringify({ email, senha }),
    parametros(null, 'autenticar'),
  );

  let token;
  try {
    token = resposta.json('token');
  } catch {
    token = null;
  }

  const autenticou = check(resposta, {
    'autenticacao retornou 200': (r) => r.status === 200,
    'autenticacao retornou token': () => typeof token === 'string' && token.length > 0,
  });

  if (!autenticou) {
    fail(`Nao foi possivel autenticar o usuario de teste. Status HTTP: ${resposta.status}.`);
  }

  return {
    baseUrl: BASE_URL,
    token,
    runId: Date.now().toString(),
  };
}

export function testarListagem(dados) {
  listarClientes(dados);
  pausar();
}

export function testarCadastro(dados) {
  cadastrarCliente(dados);
  pausar();
}

export function testarFluxo(dados) {
  const cliente = cadastrarCliente(dados);
  if (!cliente) {
    pausar();
    return;
  }

  obterClientePorId(dados, cliente.id);
  atualizarCliente(dados, cliente);
  listarClientes(dados);
  pausar();
}

export function testarFluxoAdmin(dados) {
  const cliente = cadastrarCliente(dados);
  if (!cliente) {
    pausar();
    return;
  }

  obterClientePorId(dados, cliente.id);
  atualizarCliente(dados, cliente);
  alterarAtivacao(dados, cliente.id, 'inativar', 204);
  alterarAtivacao(dados, cliente.id, 'ativar', 204);
  listarClientes(dados);
  pausar();
}

function cadastrarCliente(dados) {
  const identificador = identificadorUnico(dados.runId);
  const cliente = {
    nome: `Cliente k6 ${identificador}`,
    email: `cliente-k6-${identificador}@teste.local`,
    cpf: gerarCpfValido(dados.runId, exec.scenario.iterationInTest),
  };

  const resposta = http.post(
    `${dados.baseUrl}/api/Cliente`,
    JSON.stringify(cliente),
    parametros(dados.token, 'cadastrar'),
  );

  let id;
  try {
    id = resposta.json('id');
  } catch {
    id = null;
  }

  const cadastrou = check(resposta, {
    'cadastro retornou 201': (r) => r.status === 201,
    'cadastro retornou id': () => Number.isInteger(id) && id > 0,
  });

  if (!cadastrou) {
    return null;
  }

  return { ...cliente, id };
}

function obterClientePorId(dados, clienteId) {
  const resposta = http.get(
    `${dados.baseUrl}/api/Cliente/${clienteId}`,
    parametros(dados.token, 'obter_por_id'),
  );

  check(resposta, {
    'consulta por id retornou 200': (r) => r.status === 200,
    'consulta retornou o cliente correto': (r) => {
      try {
        return r.json('id') === clienteId;
      } catch {
        return false;
      }
    },
  });
}

function atualizarCliente(dados, cliente) {
  const corpo = {
    nome: `${cliente.nome} Atualizado`,
    email: cliente.email.replace('@', '-atualizado@'),
    cpf: cliente.cpf,
  };

  const resposta = http.put(
    `${dados.baseUrl}/api/Cliente/${cliente.id}`,
    JSON.stringify(corpo),
    parametros(dados.token, 'atualizar'),
  );

  check(resposta, {
    'atualizacao retornou 200': (r) => r.status === 200,
    'atualizacao retornou o nome alterado': (r) => {
      try {
        return r.json('nome') === corpo.nome;
      } catch {
        return false;
      }
    },
  });
}

function alterarAtivacao(dados, clienteId, acao, statusEsperado) {
  const resposta = http.put(
    `${dados.baseUrl}/api/Cliente/${clienteId}/${acao}`,
    null,
    parametros(dados.token, acao),
  );

  check(resposta, {
    [`${acao} retornou ${statusEsperado}`]: (r) => r.status === statusEsperado,
  });
}

function listarClientes(dados) {
  const resposta = http.get(
    `${dados.baseUrl}/api/Cliente`,
    parametros(dados.token, 'listar'),
  );

  check(resposta, {
    'listagem retornou 200': (r) => r.status === 200,
    'listagem retornou um array JSON': (r) => {
      try {
        return Array.isArray(r.json());
      } catch {
        return false;
      }
    },
  });
}

function parametros(token, operacao) {
  const headers = {
    'Content-Type': 'application/json',
    Accept: 'application/json',
  };

  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  return {
    headers,
    tags: { operacao },
  };
}

function identificadorUnico(runId) {
  return `${runId}-${exec.scenario.iterationInTest}-${exec.vu.idInTest}`;
}

function gerarCpfValido(runId, iteracao) {
  const deslocamentoDaExecucao = Number(runId.slice(-9));
  const sequencia = Number(iteracao) % 1_000_000_000;
  const numeroBase = (deslocamentoDaExecucao + sequencia * 104729) % 1_000_000_000;
  let base = String(numeroBase).padStart(9, '0');

  if (new Set(base).size === 1) {
    base = `12345678${base[0]}`;
  }

  const primeiroDigito = calcularDigitoCpf(base, 10);
  const segundoDigito = calcularDigitoCpf(`${base}${primeiroDigito}`, 11);
  return `${base}${primeiroDigito}${segundoDigito}`;
}

function calcularDigitoCpf(numeros, pesoInicial) {
  let soma = 0;
  for (let indice = 0; indice < numeros.length; indice += 1) {
    soma += Number(numeros[indice]) * (pesoInicial - indice);
  }

  const resto = soma % 11;
  return resto < 2 ? 0 : 11 - resto;
}

function pausar() {
  if (PAUSA_SEGUNDOS > 0) {
    sleep(PAUSA_SEGUNDOS);
  }
}

function inteiroPositivo(valor, padrao, nome) {
  if (valor === undefined || valor === '') {
    return padrao;
  }

  const numero = Number(valor);
  if (!Number.isInteger(numero) || numero <= 0) {
    throw new Error(`${nome} deve ser um numero inteiro maior que zero.`);
  }

  return numero;
}

function numeroNaoNegativo(valor, padrao, nome) {
  if (valor === undefined || valor === '') {
    return padrao;
  }

  const numero = Number(valor);
  if (!Number.isFinite(numero) || numero < 0) {
    throw new Error(`${nome} deve ser um numero maior ou igual a zero.`);
  }

  return numero;
}
