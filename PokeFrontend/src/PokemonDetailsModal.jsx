import React, { useState, useEffect } from 'react';
import axios from 'axios';

function PokemonDetailsModal({ pokemonId, onClose, apiUrl }) {
    const [details, setDetails] = useState(null);
    const [loading, setLoading] = useState(true);
    const [reviews, setReviews] = useState([]);
    const [reviewText, setReviewText] = useState('');
    const [rating, setRating] = useState(5);

    useEffect(() => {
        loadPokemonDetails(pokemonId);
    }, [pokemonId]);

    const loadPokemonDetails = async (id) => {
        if (!id) return;
        try {
            setLoading(true);
            const response = await axios.get(`${apiUrl}/${id}`);
            setDetails(response.data);

            // Dohvaćanje recenzija s tvog ReviewController-a
            try {
                const revResponse = await axios.get(`http://localhost:5033/api/Review/pokemon/${id}`);
                setReviews(revResponse.data);
            } catch (revErr) {
                setReviews([]);
            }

            setLoading(false);
        } catch (err) {
            console.error("Greška pri dohvaćanju detalja:", err);
            setLoading(false);
        }
    };

    const handleSendReview = async (e) => {
        e.preventDefault();
        const newReviewBody = {
            comment: reviewText,
            rating: parseInt(rating),
            pokemonId: details.id
        };

        try {
            await axios.post("http://localhost:5033/api/Review", newReviewBody);
            setReviewText('');
            // Osvježi listu
            const revResponse = await axios.get(`http://localhost:5033/api/Review/pokemon/${details.id}`);
            setReviews(revResponse.data);
        } catch (err) {
            alert("Za ostavljanje recenzije morate biti prijavljeni s ispravnim JWT tokenom.");
        }
    };

    const playCry = (url) => {
        if (!url) return;
        const audio = new Audio(url);
        audio.volume = 0.2;
        audio.play().catch(e => console.log("Audio block", e));
    };

    if (!pokemonId) return null;

    return (
        <div className="fixed inset-0 bg-black/90 backdrop-blur-md flex items-center justify-center p-4 z-50">
            <div className="relative flex items-center max-w-5xl w-full">

                <button
                    onClick={() => loadPokemonDetails(details?.previousId)}
                    disabled={!details?.previousId || loading}
                    className="absolute -left-16 bg-slate-900 hover:bg-red-500 disabled:opacity-20 p-4 rounded-full text-white font-black text-xl transition hidden md:block"
                >
                    ◀
                </button>

                <div className="bg-slate-900 rounded-3xl border border-slate-800 w-full overflow-hidden shadow-2xl relative max-h-[90vh] overflow-y-auto">
                    <button onClick={onClose} className="absolute top-4 right-4 bg-slate-950 hover:bg-red-500 text-slate-400 hover:text-white w-9 h-9 rounded-full flex items-center justify-center font-bold transition z-10">✕</button>

                    {loading ? (
                        <div className="p-20 text-center text-xl font-bold animate-pulse text-amber-400">Učitavam iz C# baze...</div>
                    ) : (
                        <div className="grid grid-cols-1 lg:grid-cols-2 p-6 md:p-8 gap-6">

                            <div className="flex flex-col">
                                <div className="flex flex-col items-center justify-center bg-slate-950 rounded-2xl p-6 relative border border-slate-800">
                                    <span className="absolute top-3 left-4 text-slate-600 font-mono font-black text-lg">#{String(details.id).padStart(3, '0')}</span>
                                    <img src={details.imageUrl} alt={details.name} className="h-44 w-44 object-contain" />

                                    {details.cryUrl && (
                                        <button
                                            onClick={() => playCry(details.cryUrl)}
                                            className="mt-4 bg-red-600 hover:bg-red-500 text-white font-extrabold px-5 py-1.5 rounded-full flex items-center gap-2 text-xs uppercase tracking-wider transition"
                                        >
                                            🔊 Pusti Krik
                                        </button>
                                    )}
                                </div>

                                <div className="mt-4 bg-slate-950 p-4 rounded-2xl border border-slate-800 flex-1">
                                    <h3 className="text-xs font-black text-slate-400 uppercase tracking-wider mb-2">Svi dostupni napadi ({details.moves?.length || 0}):</h3>
                                    <div className="grid grid-cols-2 gap-1 max-h-[160px] overflow-y-auto pr-1 text-[11px]">
                                        {(details.moves || []).map((m, idx) => (
                                            <span key={idx} className="bg-slate-900 px-2 py-1 rounded text-slate-300 capitalize truncate">
                                                • {m.name} <span className="text-slate-500">({m.typeName})</span>
                                            </span>
                                        ))}
                                    </div>
                                </div>
                            </div>

                            <div className="flex flex-col justify-between">
                                <div>
                                    <h2 className="text-3xl font-black capitalize tracking-wide text-white mb-2">{details.name}</h2>

                                    <div className="flex gap-2 mb-3">
                                        {details.types?.map((t, idx) => (
                                            <span key={idx} className="bg-slate-950 text-amber-400 text-[10px] uppercase font-black px-2 py-0.5 rounded border border-slate-800">{t}</span>
                                        ))}
                                    </div>

                                    <p className="text-slate-300 italic text-xs bg-slate-950 p-3 rounded-xl border border-slate-800 mb-4 leading-relaxed">
                                        "{details.description || "Nema tekstualnog opisa u bazi."}"
                                    </p>

                                    <div className="grid grid-cols-3 gap-2 mb-4">
                                        <div className="bg-slate-950 p-2 rounded-xl text-center"><span className="text-red-400 block font-black text-sm">{details.hp}</span><span className="text-[9px] text-slate-500 font-bold uppercase">HP</span></div>
                                        <div className="bg-slate-950 p-2 rounded-xl text-center"><span className="text-orange-400 block font-black text-sm">{details.attack}</span><span className="text-[9px] text-slate-500 font-bold uppercase">ATK</span></div>
                                        <div className="bg-slate-950 p-2 rounded-xl text-center"><span className="text-blue-400 block font-black text-sm">{details.defense}</span><span className="text-[9px] text-slate-500 font-bold uppercase">DEF</span></div>
                                        <div className="bg-slate-950 p-2 rounded-xl text-center"><span className="text-purple-400 block font-black text-sm">{details.spAttack}</span><span className="text-[9px] text-slate-500 font-bold uppercase">SATK</span></div>
                                        <div className="bg-slate-950 p-2 rounded-xl text-center"><span className="text-green-400 block font-black text-sm">{details.spDefense}</span><span className="text-[9px] text-slate-500 font-bold uppercase">SDEF</span></div>
                                        <div className="bg-slate-950 p-2 rounded-xl text-center"><span className="text-sky-400 block font-black text-sm">{details.speed}</span><span className="text-[9px] text-slate-500 font-bold uppercase">SPD</span></div>
                                    </div>

                                    <div className="bg-slate-950 p-4 rounded-2xl border border-slate-800 mb-4">
                                        <h4 className="text-xs font-black text-amber-400 uppercase tracking-wider mb-2">⭐⭐ Ocijeni ovog Pokémona</h4>
                                        <form onSubmit={handleSendReview} className="space-y-2">
                                            <div className="flex gap-2">
                                                <select
                                                    value={rating} onChange={(e) => setRating(e.target.value)}
                                                    className="bg-slate-900 border border-slate-800 text-xs rounded-lg p-2 text-amber-400 font-bold focus:outline-none w-full"
                                                >
                                                    {[5, 4, 3, 2, 1].map(num => <option key={num} value={num}>{num} ⭐</option>)}
                                                </select>
                                            </div>
                                            <textarea
                                                placeholder="Napiši komentar..." required rows="2"
                                                value={reviewText} onChange={(e) => setReviewText(e.target.value)}
                                                className="w-full bg-slate-900 border border-slate-800 text-xs rounded-lg p-2 text-white focus:outline-none resize-none"
                                            />
                                            <button type="submit" className="w-full bg-amber-500 hover:bg-amber-400 text-slate-950 text-xs font-black py-1.5 rounded-lg uppercase transition">
                                                Spremi Ocjenu
                                            </button>
                                        </form>
                                    </div>

                                    <div className="space-y-1.5 max-h-[140px] overflow-y-auto pr-1">
                                        <p className="text-[10px] font-black uppercase text-slate-500">Recenzije ostalih trenera:</p>
                                        {reviews.length === 0 ? (
                                            <p className="text-xs italic text-slate-600">Nema komentara.</p>
                                        ) : (
                                            reviews.map((r, i) => (
                                                <div key={i} className="bg-slate-900/60 p-2 rounded-lg border border-slate-800 text-[11px]">
                                                    <div className="flex justify-between font-bold text-slate-400 mb-0.5">
                                                        <span>👤 Trener #{r.userId}</span>
                                                        <span className="text-amber-400">{"⭐".repeat(r.rating)}</span>
                                                    </div>
                                                    <p className="text-slate-300 font-medium">{r.comment}</p>
                                                </div>
                                            ))
                                        )}
                                    </div>

                                </div>
                            </div>

                        </div>
                    )}
                </div>

                <button
                    onClick={() => loadPokemonDetails(details?.nextId)}
                    disabled={!details?.nextId || loading}
                    className="absolute -right-16 bg-slate-900 hover:bg-red-500 disabled:opacity-20 p-4 rounded-full text-white font-black text-xl transition hidden md:block"
                >
                    ▶
                </button>

            </div>
        </div>
    );
}

export default PokemonDetailsModal;