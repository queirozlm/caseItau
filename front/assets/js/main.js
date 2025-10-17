// Simula um ID incremental de reclamações
let contador = 1000;

// Lista que guarda o histórico localmente
const historico = [];

/**
 * Alterna entre as abas (Registrar / Histórico)
 */
document.getElementById('tabRegistrar').addEventListener('click', () => alternarAba('registrar'));
document.getElementById('tabHistorico').addEventListener('click', () => alternarAba('historico'));

function alternarAba(aba) {
  const registrar = document.getElementById('abaRegistrar');
  const historicoEl = document.getElementById('abaHistorico');

  if (aba === 'registrar') {
    registrar.classList.remove('hidden');
    historicoEl.classList.add('hidden');
  } else {
    registrar.classList.add('hidden');
    historicoEl.classList.remove('hidden');
    atualizarHistorico();
  }
}

/**
 * Evento principal: Envio de uma nova reclamação
 */
document.getElementById('btnEnviar').addEventListener('click', enviarReclamacao);

async function enviarReclamacao() {
  const id = contador++;
  const cpf = document.getElementById('cpfCliente').value.trim();
  const nome = document.getElementById('nomeReclamante').value.trim();
  const contato = document.getElementById('contatoReclamante').value.trim();
  const texto = document.getElementById('reclamacao').value.trim();
  const anexos = Array.from(document.getElementById('anexos').files).map(f => f.name);

  if (!nome || !contato || !texto) {
    alert('Por favor, preencha todos os campos obrigatórios.');
    return;
  }

  try {
    // 1️⃣ Envia a reclamação para a API de classificação
    const response = await fetch('http://localhost:5188/api/Classification', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ reclamacao: texto })
    });

    if (!response.ok) throw new Error(`Erro HTTP ${response.status}`);
    const categorias = await response.json();

    // 2️⃣ Busca dados do cliente no DataMesh (simulado)
    const dadosCliente = await consultarDataMesh(cpf, nome);

    // 3️⃣ Exibe o resultado visualmente
    exibirResultado({ id, cpf, nome, contato, texto, anexos, categorias, dadosCliente });

    // 4️⃣ Armazena no histórico
    adicionarAoHistorico({ id, cpf, nome, contato, texto, anexos, categorias });

    // 5️⃣ Limpa os campos
    limparCampos();
  } catch (e) {
    document.getElementById('resultado').innerHTML =
      `<div class="alert alert-danger">Erro ao enviar: ${e.message}</div>`;
  }
}

/**
 * Consulta o endpoint do DataMesh Controller
 */
async function consultarDataMesh(cpf, nome) {
  try {
    const url = cpf
      ? `http://localhost:5188/api/DataMesh/${encodeURIComponent(cpf)}`
      : `http://localhost:5188/api/DataMesh/byName?name=${encodeURIComponent(nome)}`;
    const res = await fetch(url);
    return res.ok ? await res.json() : null;
  } catch {
    return null;
  }
}

/**
 * Limpa os campos após o envio
 */
function limparCampos() {
  document.getElementById('cpfCliente').value = '';
  document.getElementById('nomeReclamante').value = '';
  document.getElementById('contatoReclamante').value = '';
  document.getElementById('reclamacao').value = '';
  document.getElementById('anexos').value = '';
}

/**
 * Exibe o resultado da classificação e os dados do cliente
 */
function exibirResultado({ id, cpf, nome, contato, texto, anexos, categorias, dadosCliente }) {
  const container = document.getElementById('resultado');

  let html = `
    <h4>📊 Resultado da Classificação</h4>
    <table class="table table-bordered">
      <tr><th>ID</th><td>${id}</td></tr>
      <tr><th>CPF</th><td>${cpf || '—'}</td></tr>
      <tr><th>Nome</th><td>${nome}</td></tr>
      <tr><th>Contato</th><td>${contato}</td></tr>
      <tr><th>Reclamação</th><td>${texto}</td></tr>
      <tr><th>Anexos</th><td>${anexos.length > 0 ? anexos.join(', ') : 'Nenhum'}</td></tr>
    </table>

    <h5>Categorias Identificadas</h5>
    <table class="table table-striped">
      <thead><tr><th>Categoria</th><th>Confiança</th><th>Palavras Encontradas</th></tr></thead>
      <tbody>
  `;

  for (const c of categorias) {
    html += `
      <tr>
        <td>${c.category}</td>
        <td>${(c.confidence * 100).toFixed(1)}%</td>
        <td>${c.matchedKeywords.join(', ')}</td>
      </tr>
    `;
  }

  html += `</tbody></table>`;

  if (dadosCliente) {
    html += `
      <h5>💻 Dados do Cliente (Data Mesh)</h5>
      <table class="table table-sm table-bordered">
        <tr><th>Nome</th><td>${dadosCliente.nome}</td></tr>
        <tr><th>Produtos</th><td>${dadosCliente.produtos?.join(', ') ?? '—'}</td></tr>
        <tr><th>Relacionamento</th><td>${dadosCliente.relacionamento ?? '—'}</td></tr>
        <tr><th>Score</th><td>${dadosCliente.score ?? '—'}</td></tr>
        <tr><th>Risco</th><td>${dadosCliente.risco ?? '—'}</td></tr>
      </table>
    `;
  }

  container.innerHTML = html;
}

/**
 * Adiciona a reclamação ao histórico
 */
function adicionarAoHistorico({ id, cpf, nome, contato, texto, anexos, categorias }) {
  const categoriaPrincipal = categorias.length > 0
    ? categorias.sort((a, b) => b.confidence - a.confidence)[0].category
    : "Não Classificada";

  historico.push({ id, cpf, nome, contato, texto, anexos, categoriaPrincipal });
}

/**
 * Atualiza a aba de histórico agrupando por tipo de demanda
 */
function atualizarHistorico() {
  const container = document.getElementById('historicoContainer');
  container.innerHTML = '';

  if (historico.length === 0) {
    container.innerHTML = `<div class="alert alert-info">Nenhuma reclamação registrada ainda.</div>`;
    return;
  }

  const agrupado = {};
  for (const item of historico) {
    if (!agrupado[item.categoriaPrincipal]) agrupado[item.categoriaPrincipal] = [];
    agrupado[item.categoriaPrincipal].push(item);
  }

  for (const categoria in agrupado) {
    let html = `
      <h4 class="mt-4 category-header">${categoria}</h4>
      <table class="table table-striped">
        <thead><tr><th>ID</th><th>CPF</th><th>Nome</th><th>Contato</th><th>Reclamação</th><th>Anexos</th></tr></thead>
        <tbody>
    `;

    for (const item of agrupado[categoria]) {
      html += `
        <tr>
          <td>${item.id}</td>
          <td>${item.cpf || '—'}</td>
          <td>${item.nome}</td>
          <td>${item.contato}</td>
          <td>${item.texto}</td>
          <td>${item.anexos.length > 0 ? item.anexos.join(', ') : '—'}</td>
        </tr>
      `;
    }

    html += `</tbody></table>`;
    container.innerHTML += html;
  }
}
