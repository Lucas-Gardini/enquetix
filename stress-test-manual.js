// Script de teste de stress manual
// Execute com: node stress-test-manual.js
// Instale as dependências: npm install node-fetch tough-cookie

const fetch = require('node-fetch');
const { CookieJar } = require('tough-cookie');

const BASE_URL = 'http://localhost:5261';
const POLL_ID = 'befa7ec1-b972-43d3-864d-7a9832c91874';

// Configurações do teste
const CONCURRENT_USERS = 5000; // 5k usuários virtuais
const DURATION_SECONDS = 60; // 1 minuto
const BATCH_SIZE = 100; // Processa 100 usuários por vez para evitar sobrecarga

// Estatísticas
let stats = {
	totalUsers: 0,
	successfulUsers: 0,
	failedUsers: 0,
	totalRequests: 0,
	successfulRequests: 0,
	failedRequests: 0,
	startTime: null,
	endTime: null
};

// Função helper para fazer requisições com suporte a cookies
async function fetchWithCookies(url, options = {}, cookieJar) {
	const cookieString = await cookieJar.getCookieString(url);

	const headers = {
		...options.headers,
		...(cookieString && { 'Cookie': cookieString })
	};

	const response = await fetch(url, {
		...options,
		headers
	});

	// Salvar cookies da resposta
	const setCookieHeader = response.headers.get('set-cookie');
	if (setCookieHeader) {
		const cookies = Array.isArray(setCookieHeader) ? setCookieHeader : [setCookieHeader];
		for (const cookie of cookies) {
			await cookieJar.setCookie(cookie, url);
		}
	}

	return response;
}

// Função para criar usuário
async function createUser() {
	const email = `testuser${Math.floor(Math.random() * 100000)}@example.com`;
	const payload = {
		email,
		username: `Test User ${Math.floor(Math.random() * 10000)}`,
		password: 'password123',
	};

	try {
		stats.totalRequests++;
		const response = await fetch(`${BASE_URL}/users`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json'
			},
			body: JSON.stringify(payload)
		});

		if (response.status === 201) {
			stats.successfulRequests++;
			return email;
		} else {
			stats.failedRequests++;
			console.error(`Erro ao criar usuário: ${response.status}`);
			return null;
		}
	} catch (error) {
		stats.failedRequests++;
		console.error('Erro ao criar usuário:', error.message);
		return null;
	}
}

// Função para fazer login
async function login(email, cookieJar) {
	const payload = new URLSearchParams();
	payload.append('email', email);
	payload.append('password', 'password123');

	try {
		stats.totalRequests++;
		const response = await fetchWithCookies(`${BASE_URL}/auth/login`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/x-www-form-urlencoded'
			},
			body: payload.toString()
		}, cookieJar);

		if (response.status === 200) {
			stats.successfulRequests++;
			return await response.json();
		} else {
			stats.failedRequests++;
			throw new Error(`Erro ao fazer login: ${response.status}`);
		}
	} catch (error) {
		stats.failedRequests++;
		console.error('Erro ao fazer login:', error.message);
		return null;
	}
}

// Função para obter detalhes da enquete
async function getPollDetails(cookieJar = null) {
	try {
		stats.totalRequests++;
		const response = cookieJar
			? await fetchWithCookies(`${BASE_URL}/polls/${POLL_ID}`, {}, cookieJar)
			: await fetch(`${BASE_URL}/polls/${POLL_ID}`);

		if (response.status === 200) {
			stats.successfulRequests++;
			return await response.json();
		} else {
			stats.failedRequests++;
			console.error(`Erro ao obter detalhes da enquete: ${response.status}`);
			return null;
		}
	} catch (error) {
		stats.failedRequests++;
		console.error('Erro ao obter detalhes da enquete:', error.message);
		return null;
	}
}

// Função para obter opções da enquete
async function getPollOptions(cookieJar = null) {
	try {
		stats.totalRequests++;
		const response = cookieJar
			? await fetchWithCookies(`${BASE_URL}/polls/${POLL_ID}/options`, {}, cookieJar)
			: await fetch(`${BASE_URL}/polls/${POLL_ID}/options`);

		if (response.status === 200) {
			stats.successfulRequests++;
			return await response.json();
		} else {
			stats.failedRequests++;
			console.error(`Erro ao obter opções da enquete: ${response.status}`);
			return null;
		}
	} catch (error) {
		stats.failedRequests++;
		console.error('Erro ao obter opções da enquete:', error.message);
		return null;
	}
}

// Função para votar na enquete
async function voteInPoll(options, cookieJar) {
	if (!options || options.length === 0) {
		console.error('Nenhuma opção disponível para voto');
		return null;
	}

	// Escolhe uma opção aleatória para votar
	const randomOption = options[Math.floor(Math.random() * options.length)];
	const payload = {
		optionId: randomOption.id,
	};

	try {
		stats.totalRequests++;
		const response = await fetchWithCookies(`${BASE_URL}/polls/${POLL_ID}/vote`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json'
			},
			body: JSON.stringify(payload)
		}, cookieJar);

		if (response.status === 200) {
			stats.successfulRequests++;
			return await response.json();
		} else {
			stats.failedRequests++;
			console.error(`Erro ao votar na enquete: ${response.status}`);
			return null;
		}
	} catch (error) {
		stats.failedRequests++;
		console.error('Erro ao votar na enquete:', error.message);
		return null;
	}
}

// Função que executa o fluxo completo para um usuário virtual
async function executeUserFlow(userId) {
	try {
		stats.totalUsers++;

		// Criar um cookieJar único para este usuário
		const cookieJar = new CookieJar();

		// 1. Criar conta de usuário
		const userEmail = await createUser();
		if (!userEmail) {
			stats.failedUsers++;
			return;
		}

		// 2. Fazer login (isso salvará os cookies de autenticação)
		const loginResult = await login(userEmail, cookieJar);
		if (!loginResult) {
			stats.failedUsers++;
			return;
		}

		// 3. Obtém a enquete (usando cookies se necessário)
		const pollDetails = await getPollDetails(cookieJar);

		// 4. Obtém as opções da enquete (usando cookies se necessário)
		const pollOptions = await getPollOptions(cookieJar);

		// 5. Vota na enquete (usando os cookies de autenticação)
		if (pollDetails && pollOptions) {
			await voteInPoll(pollOptions, cookieJar);
		}

		stats.successfulUsers++;

		// Log do progresso a cada 100 usuários
		if (stats.totalUsers % 100 === 0) {
			const elapsed = (Date.now() - stats.startTime) / 1000;
			const rate = stats.totalUsers / elapsed;
			console.log(`Usuários processados: ${stats.totalUsers}/${CONCURRENT_USERS} (${rate.toFixed(2)} users/s)`);
		}

	} catch (error) {
		stats.failedUsers++;
		console.error(`Erro no fluxo do usuário ${userId}:`, error.message);
	}
}

// Função para executar um batch de usuários
async function executeBatch(batchStart, batchSize) {
	const promises = [];
	const actualBatchSize = Math.min(batchSize, CONCURRENT_USERS - batchStart);

	for (let i = 0; i < actualBatchSize; i++) {
		const userId = batchStart + i;
		promises.push(executeUserFlow(userId));
	}

	await Promise.all(promises);
}

// Função principal do teste de stress
async function runStressTest() {
	console.log('=== INICIANDO TESTE DE STRESS ===');
	console.log(`URL Base: ${BASE_URL}`);
	console.log(`Poll ID: ${POLL_ID}`);
	console.log(`Usuários virtuais: ${CONCURRENT_USERS}`);
	console.log(`Duração: ${DURATION_SECONDS} segundos`);
	console.log(`Tamanho do batch: ${BATCH_SIZE}`);
	console.log('=====================================\n');

	stats.startTime = Date.now();

	// Executar usuários em batches para controlar a carga
	const totalBatches = Math.ceil(CONCURRENT_USERS / BATCH_SIZE);

	for (let batch = 0; batch < totalBatches; batch++) {
		const batchStart = batch * BATCH_SIZE;
		console.log(`Executando batch ${batch + 1}/${totalBatches}...`);

		await executeBatch(batchStart, BATCH_SIZE);

		// Pequena pausa entre batches para não sobrecarregar
		if (batch < totalBatches - 1) {
			await new Promise(resolve => setTimeout(resolve, 100));
		}
	}

	stats.endTime = Date.now();

	// Exibir resultados finais
	console.log('\n=== RESULTADOS FINAIS ===');
	const totalDuration = (stats.endTime - stats.startTime) / 1000;
	const userRate = stats.totalUsers / totalDuration;
	const requestRate = stats.totalRequests / totalDuration;

	console.log(`Duração total: ${totalDuration.toFixed(2)} segundos`);
	console.log(`Total de usuários: ${stats.totalUsers}`);
	console.log(`Usuários bem-sucedidos: ${stats.successfulUsers}`);
	console.log(`Usuários falharam: ${stats.failedUsers}`);
	console.log(`Taxa de sucesso de usuários: ${((stats.successfulUsers / stats.totalUsers) * 100).toFixed(2)}%`);
	console.log(`Total de requests: ${stats.totalRequests}`);
	console.log(`Requests bem-sucedidas: ${stats.successfulRequests}`);
	console.log(`Requests falharam: ${stats.failedRequests}`);
	console.log(`Taxa de sucesso de requests: ${((stats.successfulRequests / stats.totalRequests) * 100).toFixed(2)}%`);
	console.log(`Taxa de usuários/segundo: ${userRate.toFixed(2)}`);
	console.log(`Taxa de requests/segundo: ${requestRate.toFixed(2)}`);
	console.log('========================');
}

// Executar o teste se o arquivo for executado diretamente
if (require.main === module) {
	runStressTest().catch(console.error);
}

module.exports = {
	runStressTest,
	executeUserFlow,
	createUser,
	login,
	getPollDetails,
	getPollOptions,
	voteInPoll,
	stats
};
