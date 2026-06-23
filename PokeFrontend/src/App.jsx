import React, { useState, useEffect } from 'react';
import axios from 'axios';
import PokemonDetailsModal from './PokemonDetailsModal';

function parseJwt(token) {
  try {
    return JSON.parse(atob(token.split('.')[1]));
  } catch (e) {
    return null;
  }
}

const STVARNI_SWAGGER_JWT_TOKEN = localStorage.getItem('token') || "";

function App() {
  const [pokemons, setPokemons] = useState([]);
  const [selectedPokemon, setSelectedPokemon] = useState(null);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('pokedex');
  const [userToken, setUserToken] = useState(STVARNI_SWAGGER_JWT_TOKEN);
  const [isAdmin, setIsAdmin] = useState(false);
  const [username, setUsername] = useState('Gostujući Trener');
  const [jwtInput, setJwtInput] = useState('');
  const [stats, setStats] = useState({ totalTeams: 0, totalReviews: 0, totalPokemons: 0, totalMoves: 0, totalItems: 0 });
  const [team, setTeam] = useState([]);
  const [teamName, setTeamName] = useState("Moj Legendarni Sastav");
  const [builderAnalysis, setBuilderAnalysis] = useState(null);
  const [loadingAnalysis, setLoadingAnalysis] = useState(false);
  const [backendError, setBackendError] = useState(null);
  const [savedTeams, setSavedTeams] = useState([]);
  const [savedTeamAnalysis, setSavedTeamAnalysis] = useState({});
  const [activeMoveSelectorIdx, setActiveMoveSelectorIdx] = useState(null);
  const [availableMoves, setAvailableMoves] = useState([]);
  const [loadingMoves, setLoadingMoves] = useState(false);

  const API_URL = "http://localhost:5033/api/Poke";
  const TEAM_API_URL = "http://localhost:5033/api/Team";

  useEffect(() => {
    if (userToken) {
      axios.defaults.headers.common['Authorization'] = `Bearer ${userToken}`;
      localStorage.setItem('token', userToken);
      const decoded = parseJwt(userToken);
      if (decoded) {
        const role = decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || decoded["role"];
        setIsAdmin(role === "Admin");
        const name = decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || "Trener";
        setUsername(name);
      }
    } else {
      delete axios.defaults.headers.common['Authorization'];
      localStorage.removeItem('token');
      setIsAdmin(false);
      setUsername('Gostujući Trener');
    }
  }, [userToken]);

  useEffect(() => {
    fetchPokemons();
    fetchStats();
    if (userToken) {
      fetchSavedTeams();
    }
  }, [userToken]);

  const fetchPokemons = async () => {
    try {
      setLoading(true);
      const response = await axios.get(API_URL);
      setPokemons(response.data);
      setLoading(false);
    } catch (err) {
      console.error(err);
      setLoading(false);
    }
  };

  const fetchStats = async () => {
    try {
      const response = await axios.get("http://localhost:5033/api/Stats");
      setStats(response.data);
    } catch (err) {
      console.error(err);
    }
  };

  const fetchSavedTeams = async () => {
    try {
      const response = await axios.get(`${TEAM_API_URL}/my-teams`);
      setSavedTeams(response.data);
    } catch (err) {
      console.error(err);
    }
  };

  const handleJwtLogin = (e) => {
    e.preventDefault();
    if (!jwtInput) return;
    const cleanToken = jwtInput.replace("Bearer ", "").trim();
    setUserToken(cleanToken);
    setJwtInput('');
    alert("Token uspješno postavljen!");
  };

  const handleLogout = () => {
    setUserToken('');
    setSavedTeams([]);
    setTeam([]);
    setBuilderAnalysis(null);
    setSavedTeamAnalysis({});
    setActiveTab('pokedex');
  };

  const addToTeam = (pokemon) => {
    if (team.length >= 6) return alert("Maksimalno 6 Pokémona!");
    if (team.some(t => t.pokemon.id === pokemon.id)) return alert("Pokémon je već u timu!");
    setTeam([...team, { pokemon: pokemon, selectedMoves: [] }]);
    setBuilderAnalysis(null);
  };

  const removeFromTeam = (index) => {
    setTeam(team.filter((_, i) => i !== index));
    setBuilderAnalysis(null);
  };

  const openMoveSelector = async (slotIndex, pokemonId) => {
    setActiveMoveSelectorIdx(slotIndex);
    try {
      setLoadingMoves(true);
      const response = await axios.get(`${API_URL}/${pokemonId}`);
      setAvailableMoves(response.data.moves || []);
      setLoadingMoves(false);
    } catch (err) {
      setAvailableMoves([]);
      setLoadingMoves(false);
    }
  };

  const toggleMoveForPokemon = (slotIdx, move) => {
    const updatedTeam = [...team];
    const currentMoves = updatedTeam[slotIdx].selectedMoves;
    const exists = currentMoves.some(m => m.name === move.name);

    if (exists) {
      updatedTeam[slotIdx].selectedMoves = currentMoves.filter(m => m.name !== move.name);
    } else {
      if (currentMoves.length >= 4) return alert("Maksimalno 4 napada!");
      updatedTeam[slotIdx].selectedMoves = [...currentMoves, move];
    }
    setTeam(updatedTeam);
    setBuilderAnalysis(null);
  };

  const analyzeTeamOnBackend = async () => {
    if (team.length === 0) return alert("Dodaj Pokemone u tim!");
    try {
      setLoadingAnalysis(true);
      setBackendError(null);

      const requestBody = {
        name: teamName,
        members: team.map(member => ({
          pokemonId: member.pokemon.id,
          moveIds: member.selectedMoves.map(m => m.id || m.Id || (m.name.charCodeAt(0) % 100) + 1)
        }))
      };

      const response = await axios.post(`${TEAM_API_URL}/analyze`, requestBody);
      setBuilderAnalysis(response.data);
      setLoadingAnalysis(false);
    } catch (err) {
      setBackendError("Greška pri analizi novog tima.");
      setLoadingAnalysis(false);
    }
  };

  const analyzeSavedTeamOnBackend = async (teamId) => {
    try {
      const response = await axios.post(`${TEAM_API_URL}/analyze-saved/${teamId}`);
      setSavedTeamAnalysis(prev => ({
        ...prev,
        [teamId]: response.data
      }));
    } catch (err) {
      alert("C# Backend nije uspio analizirati ovaj spremljeni tim.");
    }
  };

  const saveTeamToBackend = async () => {
    if (team.length === 0) return alert("Dodaj Pokemone!");
    try {
      const requestBody = {
        name: teamName,
        members: team.map(member => ({
          pokemonId: member.pokemon.id,
          moveIds: member.selectedMoves.map(m => m.id || m.Id || 1)
        }))
      };
      await axios.post(TEAM_API_URL, requestBody);
      alert("Tim spremljen u bazu!");
      setTeam([]);
      setBuilderAnalysis(null);
      fetchSavedTeams();
      fetchStats();
    } catch (err) {
      alert("Greška pri spremanju.");
    }
  };

  const deleteTeam = async (teamId) => {
    if (!window.confirm("Obrisati tim?")) return;
    try {
      await axios.delete(`${TEAM_API_URL}/${teamId}`);
      fetchSavedTeams();
      fetchStats();
    } catch (err) {
      alert("Greška pri brisanju.");
    }
  };

  const triggerSync = async (endpoint) => {
    try {
      alert(`Pokrećem: ${endpoint}...`);
      await axios.get(`${API_URL}/${endpoint}`);
      alert("Sinkronizacija završena!");
      fetchPokemons();
      fetchStats();
    } catch (err) {
      console.log(err);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-[#0a0f0d] text-emerald-400 flex items-center justify-center text-2xl font-black animate-pulse">
        UČITAVAM PODATKE IZ C# BAZE... 🟢
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#0a0f0d] text-[#e2ede8] p-6 font-sans">

      {/* VRH I LOGIN */}
      <div className="max-w-7xl mx-auto flex flex-col md:flex-row justify-between items-center bg-[#111a16] p-4 rounded-2xl border border-[#1b2b24] gap-4 mb-6">
        <div className="flex items-center gap-3">
          <div className="w-3 h-3 rounded-full bg-emerald-500 animate-ping"></div>
          <span className="text-xs font-bold uppercase tracking-wider text-emerald-400/70">
            {userToken ? `Prijavljen kao: ${username} ${isAdmin ? '(👑 Admin Mod)' : '(Trener)'}` : 'Gost (Umetni JWT token)'}
          </span>
        </div>
        <form onSubmit={handleJwtLogin} className="flex gap-2 w-full md:w-auto">
          <input
            type="text" placeholder="Zalijepi Bearer JWT Token iz Swaggera..."
            value={jwtInput} onChange={(e) => setJwtInput(e.target.value)}
            className="bg-[#0a0f0d] border border-[#1b2b24] rounded-lg px-3 py-1.5 text-xs text-white focus:outline-none flex-1 md:w-80"
          />
          <button type="submit" className="bg-emerald-600 hover:bg-emerald-500 text-white px-4 py-1.5 rounded-lg text-xs font-bold transition">Postavi Token</button>
          {userToken && <button type="button" onClick={handleLogout} className="bg-red-600/20 text-red-400 hover:bg-red-600 hover:text-white px-3 py-1.5 rounded-lg text-xs font-bold transition">Odjava</button>}
        </form>
      </div>

      {/* NAVIGACIJA */}
      <header className="max-w-7xl mx-auto text-center my-4">
        <h1 className="text-5xl font-black tracking-wider uppercase bg-gradient-to-r from-emerald-400 via-teal-300 to-emerald-500 bg-clip-text text-transparent">PokéApp</h1>
        <div className="flex flex-wrap justify-center gap-3 mt-6">
          <button onClick={() => setActiveTab('pokedex')} className={`px-5 py-3 rounded-xl font-black text-xs uppercase transition ${activeTab === 'pokedex' ? 'bg-emerald-600 text-white shadow-lg' : 'bg-[#111a16] text-emerald-600/70'}`}>📕 Pokédex & Ocjene</button>
          <button onClick={() => setActiveTab('teambuilder')} className={`px-5 py-3 rounded-xl font-black text-xs uppercase transition ${activeTab === 'teambuilder' ? 'bg-emerald-700 text-white shadow-lg' : 'bg-[#111a16] text-emerald-600/70'}`}>⚔️ Sastavi i Spremi Tim</button>
          <button onClick={() => setActiveTab('myteams')} className={`px-5 py-3 rounded-xl font-black text-xs uppercase transition ${activeTab === 'myteams' ? 'bg-[#a3d9c9] text-slate-950 shadow-lg' : 'bg-[#111a16] text-emerald-600/70'}`}>🗂️ Moji Spremljeni Timovi ({savedTeams.length})</button>
          <button onClick={() => setActiveTab('admin')} className={`px-5 py-3 rounded-xl font-black text-xs uppercase transition ${activeTab === 'admin' ? 'bg-teal-600 text-white shadow-lg' : 'bg-[#111a16] text-teal-400/70'}`}>👑 Admin Nadzorna Ploča</button>
        </div>
      </header>

      {/* POKEDEX */}
      {activeTab === 'pokedex' && (
        <main className="max-w-7xl mx-auto grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6 mt-6">
          {pokemons.map((poke) => (
            <div key={poke.id} className="bg-[#111a16] rounded-2xl p-5 border border-[#1b2b24] flex flex-col justify-between group">
              <div onClick={() => setSelectedPokemon(poke.id)} className="cursor-pointer">
                <div className="relative bg-[#0a0f0d] rounded-xl p-4 flex items-center justify-center h-40 mb-4">
                  <span className="absolute top-2 left-3 text-emerald-800 font-mono text-xs">#{String(poke.id).padStart(3, '0')}</span>
                  <img src={poke.imageUrl} alt={poke.name} className="h-full object-contain" />
                </div>
                <h2 className="text-xl font-black capitalize text-[#e2ede8] group-hover:text-emerald-400">{poke.name}</h2>
              </div>
              <button onClick={() => addToTeam(poke)} className="w-full bg-[#0a0f0d] hover:bg-emerald-600 text-emerald-400/80 hover:text-white font-bold py-2 rounded-xl text-xs uppercase transition mt-2">➕ Dodaj u Tim</button>
            </div>
          ))}
        </main>
      )}

      {/* TEAM BUILDER + REZULTAT ODMAH ISPOD */}
      {activeTab === 'teambuilder' && (
        <main className="max-w-5xl mx-auto mt-6">
          <section className="bg-[#111a16] border border-[#1b2b24] rounded-3xl p-6 mb-4 shadow-xl">
            <div className="flex flex-col sm:flex-row justify-between items-center mb-6 gap-4">
              <h2 className="text-2xl font-black uppercase text-emerald-400">Izrada Novog Sastava</h2>
              <input type="text" value={teamName} onChange={(e) => setTeamName(e.target.value)} className="bg-[#0a0f0d] border border-[#1b2b24] rounded-xl px-4 py-2 text-sm font-bold text-[#a3d9c9] focus:outline-none" />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {[...Array(6)].map((_, index) => {
                const member = team[index];
                return (
                  <div key={index} className="bg-[#0a0f0d] rounded-2xl p-4 border border-[#1b2b24] flex flex-col justify-between min-h-[180px] relative">
                    {member ? (
                      <>
                        <button onClick={() => removeFromTeam(index)} className="absolute top-2 right-2 bg-[#111a16] hover:bg-red-600 text-emerald-600/50 hover:text-white w-7 h-7 rounded-full text-xs font-bold">✕</button>
                        <div className="flex gap-4 items-center">
                          <img src={member.pokemon.imageUrl} alt={member.pokemon.name} className="h-16 w-16 object-contain bg-[#111a16] p-2 rounded-xl" />
                          <h3 className="font-black capitalize text-slate-100">{member.pokemon.name}</h3>
                        </div>
                        <div className="my-3 grid grid-cols-2 gap-1">
                          {member.selectedMoves.map((m, i) => <span key={i} className="bg-[#111a16] border border-[#1b2b24] text-[#a3d9c9] px-2 py-1 rounded text-[11px] font-bold truncate">💥 {m.name}</span>)}
                        </div>
                        <button onClick={() => openMoveSelector(index, member.pokemon.id)} className="w-full bg-emerald-600/10 hover:bg-emerald-600 text-emerald-400 hover:text-white font-bold py-1.5 rounded-lg text-xs uppercase transition">🛠️ Postavi Napade</button>
                      </>
                    ) : <div className="flex items-center justify-center h-full text-emerald-950 font-black py-10">+ Prazan utor</div>}
                  </div>
                );
              })}
            </div>

            <div className="mt-6 flex justify-center gap-4 border-t border-[#1b2b24]/60 pt-4">
              <button onClick={analyzeTeamOnBackend} disabled={team.length === 0} className="bg-[#0a0f0d] hover:bg-[#111a16] text-white font-black px-6 py-3 rounded-xl uppercase text-xs border border-[#1b2b24]">{loadingAnalysis ? "Analiziram..." : "⚡ Pokreni C# Analizu"}</button>
              <button onClick={saveTeamToBackend} disabled={team.length === 0} className="bg-gradient-to-r from-emerald-600 to-teal-600 text-white font-black px-8 py-3 rounded-xl uppercase text-xs shadow-xl">💾 Spremi Tim u Profil</button>
            </div>
          </section>

          {/* EKSPANZIJA: ANALIZA BUILDERA ODMAH ISPOD BUILDERA */}
          {backendError && <div className="bg-red-600/20 text-red-400 p-4 rounded-xl border border-red-500 mb-4">{backendError}</div>}
          {builderAnalysis && (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 bg-[#111a16] p-5 rounded-2xl border border-[#1b2b24]">
              <div>
                <h4 className="text-emerald-400 font-black text-xs uppercase mb-2">💥 Ofenzivna Pokrivenost:</h4>
                <div className="flex flex-wrap gap-1">
                  {(builderAnalysis.strongAgainst || builderAnalysis.StrongAgainst || []).map((type, i) => <span key={i} className="bg-emerald-500/10 text-emerald-400 px-2 py-1 rounded text-xs uppercase border border-emerald-500/20">{type}</span>)}
                  {(builderAnalysis.strongAgainst || builderAnalysis.StrongAgainst || []).length === 0 && <span className="text-xs text-emerald-700/60 italic">Nema ofenzivnih prednosti. Promijeni napade!</span>}
                </div>
              </div>
              <div>
                <h4 className="text-red-400 font-black text-xs uppercase mb-2">🛡️ Defenzivne Slabosti:</h4>
                <div className="flex flex-wrap gap-1">
                  {(builderAnalysis.weakAgainst || builderAnalysis.WeakAgainst || []).map((type, i) => <span key={i} className="bg-red-500/10 text-red-400 px-2 py-1 rounded text-xs uppercase border border-red-500/20">{type}</span>)}
                  {(builderAnalysis.weakAgainst || builderAnalysis.WeakAgainst || []).length === 0 && <span className="text-xs text-emerald-400 italic">Savršen balans, nema slabosti!</span>}
                </div>
              </div>
            </div>
          )}
        </main>
      )}

      {/* SPREMLJENI TIMOVI + INSTANT ANALIZA ODMAH ISPOD KARTICE TIMA */}
      {activeTab === 'myteams' && (
        <main className="max-w-5xl mx-auto mt-6 space-y-4">
          <h2 className="text-2xl font-black uppercase text-[#a3d9c9] mb-2">Spremljene postave u bazi</h2>
          {savedTeams.length === 0 ? (
            <div className="bg-[#111a16] rounded-2xl p-8 text-center text-emerald-700/60 border border-[#1b2b24]">Nema spremljenih timova.</div>
          ) : (
            savedTeams.map((savedTeam) => {
              const trenutnaAnaliza = savedTeamAnalysis[savedTeam.id];
              return (
                <div key={savedTeam.id} className="bg-[#111a16] p-5 rounded-2xl border border-[#1b2b24] space-y-4">
                  <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
                    <div className="flex-1">
                      <h3 className="text-lg font-black text-white capitalize">{savedTeam.name}</h3>
                      <div className="flex gap-2 mt-2 overflow-x-auto py-1">
                        {(savedTeam.members || []).map((m, idx) => {
                          const imgUrl = m.pokemonImage || m.PokemonImage;
                          const pName = m.pokemonName || m.PokemonName;
                          return (
                            <div key={idx} className="bg-[#0a0f0d] p-2 rounded-xl border border-[#1b2b24]/60 flex items-center gap-2 min-w-[140px]">
                              {imgUrl && <img src={imgUrl} className="w-8 h-8 object-contain" />}
                              <span className="text-xs font-bold capitalize text-emerald-300 truncate">{pName}</span>
                            </div>
                          );
                        })}
                      </div>
                    </div>
                    <div className="flex gap-2">
                      <button onClick={() => analyzeSavedTeamOnBackend(savedTeam.id)} className="bg-emerald-600/20 hover:bg-emerald-600 border border-emerald-500/30 text-emerald-400 hover:text-white px-4 py-2 rounded-xl text-xs font-black uppercase tracking-wider transition">⚡ Analiziraj</button>
                      <button onClick={() => deleteTeam(savedTeam.id)} className="bg-red-600/10 hover:bg-red-600 border border-red-500/20 text-red-400 hover:text-white px-4 py-2 rounded-xl text-xs font-black uppercase tracking-wider transition">Ukloni</button>
                    </div>
                  </div>

                  {/* ISPIS ANALIZE ODMAH ISPOD TIMA BEZ SKAKANJA NA DRUGI TAB */}
                  {trenutnaAnaliza && (
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4 bg-[#0a0f0d] p-4 rounded-xl border border-[#1b2b24]/80 text-xs animate-fadeIn">
                      <div>
                        <span className="text-emerald-400 font-black block uppercase tracking-wider mb-1.5">💥 Ofenzivna Prednost ovog tima:</span>
                        <div className="flex flex-wrap gap-1">
                          {(trenutnaAnaliza.strongAgainst || trenutnaAnaliza.StrongAgainst || []).map((t, i) => <span key={i} className="bg-emerald-500/10 text-emerald-400 px-2 py-0.5 rounded border border-emerald-500/10 uppercase font-bold">{t}</span>)}
                          {(trenutnaAnaliza.strongAgainst || trenutnaAnaliza.StrongAgainst || []).length === 0 && <span className="text-emerald-700/60 italic">Nema upisanih ofenzivnih prednosti.</span>}
                        </div>
                      </div>
                      <div>
                        <span className="text-red-400 font-black block uppercase tracking-wider mb-1.5">🛡️ Defenzivne slabosti ovog tima:</span>
                        <div className="flex flex-wrap gap-1">
                          {(trenutnaAnaliza.weakAgainst || trenutnaAnaliza.WeakAgainst || []).map((t, i) => <span key={i} className="bg-red-500/10 text-red-400 px-2 py-0.5 rounded border border-red-500/10 uppercase font-bold">{t}</span>)}
                          {(trenutnaAnaliza.weakAgainst || trenutnaAnaliza.WeakAgainst || []).length === 0 && <span className="text-emerald-400 italic">Nema izraženih slabosti. Sastav je balansiran!</span>}
                        </div>
                      </div>
                    </div>
                  )}
                </div>
              );
            })
          )}
        </main>
      )}

      {/* ADMIN DASHBOARD */}
      {activeTab === 'admin' && (
        <main className="max-w-4xl mx-auto mt-6">
          <div className="bg-[#111a16] border border-teal-900/40 rounded-3xl p-6">
            <h2 className="text-2xl font-black text-teal-400 uppercase mb-4">Globalna Statistika Baze</h2>
            <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-4 mb-6">
              <div className="bg-[#0a0f0d] p-4 rounded-xl text-center"><span className="block text-2xl font-black text-[#a3d9c9]">{stats.totalPokemons}</span><span className="text-[10px] text-emerald-700 font-bold uppercase">Pokemona</span></div>
              <div className="bg-[#0a0f0d] p-4 rounded-xl text-center"><span className="block text-2xl font-black text-emerald-400">{stats.totalTeams}</span><span className="text-[10px] text-emerald-700 font-bold uppercase">Timova</span></div>
              <div className="bg-[#0a0f0d] p-4 rounded-xl text-center"><span className="block text-2xl font-black text-red-400">{stats.totalReviews}</span><span className="text-[10px] text-emerald-700 font-bold uppercase">Recenzija</span></div>
              <div className="bg-[#0a0f0d] p-4 rounded-xl text-center"><span className="block text-2xl font-black text-teal-400">{stats.totalMoves}</span><span className="text-[10px] text-emerald-700 font-bold uppercase">Poteza</span></div>
              <div className="bg-[#0a0f0d] p-4 rounded-xl text-center"><span className="block text-2xl font-black text-emerald-500">{stats.totalItems}</span><span className="text-[10px] text-emerald-700 font-bold uppercase">Itema</span></div>
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <button onClick={() => triggerSync('sync')} className="bg-[#0a0f0d] hover:bg-emerald-900/20 border border-[#1b2b24] p-3 rounded-xl text-left text-xs font-bold text-emerald-400">🔄 Sync Pokemons (1025)</button>
              <button onClick={() => triggerSync('sync-types')} className="bg-[#0a0f0d] hover:bg-emerald-900/20 border border-[#1b2b24] p-3 rounded-xl text-left text-xs font-bold text-emerald-400">🏷️ Sync Element Types</button>
              <button onClick={() => triggerSync('sync-moves')} className="bg-[#0a0f0d] hover:bg-emerald-900/20 border border-[#1b2b24] p-3 rounded-xl text-left text-xs font-bold text-emerald-400">⚔️ Sync Attack Catalog</button>
              <button onClick={() => triggerSync('sync-pokemon-moves')} className="bg-[#0a0f0d] hover:bg-emerald-900/20 border border-[#1b2b24] p-3 rounded-xl text-left text-xs font-bold text-emerald-400">🧬 Sync Attack Relations</button>
            </div>
          </div>
        </main>
      )}

      {/* MOVE MODAL */}
      {activeMoveSelectorIdx !== null && (
        <div className="fixed inset-0 bg-black/85 backdrop-blur-sm flex items-center justify-center p-4 z-50">
          <div className="bg-[#111a16] border border-[#1b2b24] rounded-3xl p-6 max-w-2xl w-full max-h-[80vh] flex flex-col justify-between">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-xl font-black text-[#a3d9c9] uppercase">Dostupni napadi:</h3>
              <button onClick={() => setActiveMoveSelectorIdx(null)} className="text-emerald-600 text-xl font-bold">✕</button>
            </div>
            {loadingMoves ? <div className="py-20 text-center font-bold animate-pulse text-emerald-400">Učitavam napade...</div> : (
              <div className="overflow-y-auto pr-2 grid grid-cols-2 sm:grid-cols-3 gap-2 max-h-[50vh]">
                {availableMoves.map((move, index) => {
                  const isSelected = team[activeMoveSelectorIdx]?.selectedMoves.some(m => m.name === move.name);
                  return (
                    <button key={index} onClick={() => toggleMoveForPokemon(activeMoveSelectorIdx, move)} className={`p-2 rounded-xl border text-xs text-left transition flex flex-col justify-between ${isSelected ? 'bg-emerald-600 border-emerald-400 text-white shadow-lg' : 'bg-[#0a0f0d] border-[#1b2b24] text-emerald-300'}`}>
                      <span className="font-extrabold capitalize">{move.name}</span>
                      <span className="text-[10px] uppercase font-black text-[#a3d9c9] mt-1">{move.typeName}</span>
                    </button>
                  );
                })}
              </div>
            )}
            <div className="border-t border-[#1b2b24] pt-4 text-right">
              <button onClick={() => setActiveMoveSelectorIdx(null)} className="bg-emerald-600 text-white font-black px-6 py-2 rounded-xl text-xs uppercase">Završi</button>
            </div>
          </div>
        </div>
      )}

      {selectedPokemon && <PokemonDetailsModal pokemonId={selectedPokemon} onClose={() => setSelectedPokemon(null)} apiUrl={API_URL} />}
    </div>
  );
}

export default App;

